<template>
  <b-container v-if="currentVersion.version">
    <b-list-group>
      <b-list-group-item v-for="deployment in deployments" :key="deployment.id" class="list-group-item">
        <b-container>
          <b-row align-content="end">
            <b-col class="text-right">Deployment Id:</b-col>
            <b-col align-content="start" class="text-left"
              >{{ deployment.id }}
              <b-button size="sm" class="btn float-right close" @click="onDeploymentDelete(deployment.id)"
                ><b-icon icon="x"></b-icon></b-button
            ></b-col>
          </b-row>
          <b-row>
            <b-col class="text-right">Publish Date:</b-col>
            <b-col class="text-left">{{ formatPublishDate(deployment.publishDate) }}</b-col>
          </b-row>

          <b-row>
            <b-col class="text-right">Client Id:</b-col>
            <b-col class="text-left">{{ deployment.clientId }}</b-col>
          </b-row>
          <b-row>
            <b-col class="text-right">Client Type:</b-col>
            <b-col class="text-left">{{ deployment.clientType }}</b-col>
          </b-row>
          <b-row>
            <b-col class="text-right">Alias:</b-col>
            <b-col class="text-left">{{ deployment.alias }}</b-col>
          </b-row>
        </b-container>
      </b-list-group-item>
    </b-list-group>
    <b-modal
      id="confirmDeleteModal"
      ref="confirmDeleteModal"
      size="sm"
      title="Delete Deployment"
      @ok="handleDelete"
      footer-bg-variant="warning"
      button-size="sm"
      ok-title="Delete"
      centered
    >
      <div class="d-block text-center">Delete deployment {{ deploymentToDelete.id }}?</div>
    </b-modal>
  </b-container>
</template>

<script lang="ts">
// tslint:disable-next-line
import { format, parseISO } from 'date-fns';
import { inputDateFormat } from '@/shared/constants';
import { VersionModule } from '@/store/modules/versionmanager';
import { DeploymentModule } from '@/store/modules/deploymentmanager';
import { IVersion, IDeployment, IDeleteDeploymentRequest, logger } from '@/shared';
import { BModal } from 'bootstrap-vue';

import { Component, Watch, Ref, Vue } from 'vue-property-decorator';

@Component({ name: 'DeploymentsList' })
export default class DeploymentsList extends Vue {
  private deploymentToDeleteInternal: IDeployment = {
    id: undefined,
    projectId: undefined,
    versionId: undefined,
    clientType: 'unknown',
    clientId: undefined,
    publishDate: undefined,
    alias: undefined
  };

  get currentVersion(): IVersion {
    return VersionModule.currentVersion;
  }

  get deployments(): IDeployment[] {
    return DeploymentModule.deployments;
  }

  get deploymentToDelete(): IDeployment {
    if (this.deploymentToDeleteInternal.id) return this.deploymentToDeleteInternal;
    else {
      const retDeploymemt: IDeployment = {
        id: undefined,
        projectId: undefined,
        versionId: undefined,
        clientType: 'unknown',
        clientId: undefined,
        publishDate: undefined,
        alias: undefined
      };
      return retDeploymemt;
    }
  }

  @Ref()
  private readonly confirmDeleteModal!: BModal;

  public formatPublishDate(publishDate: Date | undefined): string | undefined {
    if (publishDate) return format(parseISO(publishDate.toString()), inputDateFormat);

    return '';
  }

  public onDeploymentDelete(deploymentId: string): void {
    this.deploymentToDeleteInternal = this.deployments.find((x) => deploymentId === x.id) as IDeployment;
    this.confirmDeleteModal.show();
  }

  public async handleDelete(): Promise<void> {
    if (this.deploymentToDelete.id) {
      const curVer = VersionModule.currentVersion;

      if (curVer?.id && curVer?.projectId) {
        const deleteReq: IDeleteDeploymentRequest = {
          projectId: curVer.projectId,
          versionId: curVer.id,
          deploymentId: this.deploymentToDelete.id
        };

        await DeploymentModule.deleteDeploymentAsync(deleteReq);
      } else {
        throw new Error('Current version is not set and cannot be deleted');
      }
    }
  }

  public async onSave() {
    await VersionModule.saveCurrentVersion();
  }

  @Watch('currentVersion', { immediate: true })
  public async onVersionChanged(val: IVersion, oldVal: IVersion) {
    if (val.version) {
      logger.log(`version id: ${val.id}`);
      // clear the deployments array.
      DeploymentModule.deployments.splice(0, DeploymentModule.deployments.length);
      await DeploymentModule.loadDeploymentsAsync(val);
    } else logger.log('no new value?');

    if (oldVal) logger.log(`old val in version details: ${oldVal.version}`);
  }
}
</script>
