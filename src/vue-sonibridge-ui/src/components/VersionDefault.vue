<template>
  <div>
    <VersionDetails />
    <b-button
      v-if="currentVersion.version"
      type="button"
      class="btn m-md-1 float-right btn-sm"
      variant="primary"
      @click="onShowNewDeployment()"
      >New Deployment</b-button
    >
    <NewDeployment ref="newDeployment" />
    <DeploymentsList />
  </div>
</template>

<script lang="ts">
// @ is an alias to /src
import VersionDetails from '@/components/VersionDetails.vue';
import NewDeployment from '@/components/NewDeployment.vue';
import DeploymentsList from '@/components/DeploymentsList.vue';
import { Component, Ref, Vue } from 'vue-property-decorator';
import { IVersion } from '@/shared';
import { VersionModule } from '@/store/modules/versionmanager';

@Component({ name: 'VersionDefault', components: { VersionDetails, DeploymentsList, NewDeployment } })
export default class VersionDefault extends Vue {
  get currentVersion(): IVersion {
    return VersionModule.currentVersion;
  }

  // This provides programmatic access to the NewDeployment instance on the view
  // when used in tandem with the ref attribute on the element. (see the NewDeployment tag in the template)
  @Ref()
  private readonly newDeployment!: NewDeployment;

  public onShowNewDeployment(): void {
    this.newDeployment.show(
      this.currentVersion?.projectId ? this.currentVersion.projectId : 'unknown project id',
      this.currentVersion?.id ? this.currentVersion.id : 'unknown version id'
    );
  }
}
</script>
