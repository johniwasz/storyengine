<template>
  <div>
    <b-nav-item-dropdown
      id="lstVersions"
      name="lstVersions"
      v-model="this.currentVersion"
      variant="primary"
      class="m-md-2"
      right
      v-bind:text="this.versionDisplayText"
    >
      <b-dropdown-item disabled value="0">{{ this.versionLabel }}</b-dropdown-item>
      <b-dropdown-item
        v-for="version in this.versions"
        :key="version.id"
        :value="version.description"
        @click="changeVersion(version)"
        >{{ version.version }}</b-dropdown-item
      >
    </b-nav-item-dropdown>
  </div>
</template>

<script lang="ts">
import { Component, Watch, Vue } from 'vue-property-decorator';
import { IVersion, IProject, logger } from '@/shared';
import { VersionModule } from '@/store/modules/versionmanager';
import { ProjectModule } from '@/store/modules/projectmanager';

@Component({
  name: 'VersionList'
})
export default class VersionList extends Vue {
  // tslint:disable-next-line
  private versionLabel = 'Select a Version';

  // tslint:disable-next-line
  public versionDisplayText = this.versionLabel;

  get versions(): IVersion[] {
    return VersionModule.versions;
  }

  get currentVersion(): IVersion {
    return VersionModule.currentVersion;
  }

  set currentVersion(newVersion: IVersion) {
    VersionModule.updateCurrentVersion(newVersion);
  }

  public changeVersion(version: IVersion) {
    this.currentVersion = version;
  }

  get currentProject() {
    return ProjectModule.currentProject;
  }

  @Watch('currentVersion', { immediate: true })
  public async onVersionChanged(val: IVersion, oldVal: IVersion) {
    this.versionDisplayText = this.versionLabel;
    if (val.version) {
      logger.log(`new value in version: ${val.version}`);

      this.versionDisplayText = val.version;
    } else {
      logger.log('no selected version');
      this.versionDisplayText = this.versionLabel;
    }

    if (oldVal) logger.log(`old version: ${oldVal.version}`);
  }

  @Watch('currentProject', { immediate: true })
  public async onProjectChanged(val: IProject, oldVal: IProject) {
    this.versionDisplayText = this.versionLabel;
    if (val) {
      logger.log(`new value in version: ${val.shortName}`);
      if (val.id) await VersionModule.loadVersionsAsync(val.id);
    } else logger.log('no new value?');

    if (oldVal) logger.log(`old val: ${oldVal.shortName}`);

    const curVer: IVersion = {
      id: undefined,
      projectId: undefined,
      version: undefined,
      description: undefined,
      logFullClientMessages: undefined
    };

    this.changeVersion(curVer);
  }
}
</script>
