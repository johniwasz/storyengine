<template>
  <b-container v-if="currentVersion.version" fluid>
    <b-row>
      <h3>{{ versionTitle }}</h3>
    </b-row>
    <b-container class="border border-primary">
      <b-form-group
        id="fieldset-1"
        label="Version Id:"
        label-for="idText"
        label-cols-sm="4"
        label-cols-lg="3"
        align-h="end"
      >
        <b-form-input
          align-h="start"
          id="descriptionText"
          v-model="currentVersion.id"
          type="text"
          readonly
          required
          placeholder="description"
          >{{ currentVersion.id }}</b-form-input
        >
      </b-form-group>

      <b-form-group
        id="fieldset-2"
        description="For internal use. This is not pushed to Google Home or Alexa Skill configurations."
        label="Description:"
        label-for="descriptionText"
        label-cols-sm="4"
        label-cols-lg="3"
        align-h="start"
      >
        <b-form-textarea
          id="descriptionText"
          v-model="currentVersion.description"
          type="textarea"
          required
          placeholder="description"
          >{{ currentVersion.description }}</b-form-textarea
        >
      </b-form-group>

      <b-form-group
        id="fieldset-3"
        description="Used for diagnostics. Disable if privacy is a concern."
        label-for="logFullClientMessagesCheckbox"
        label-cols-sm="4"
        label-cols-lg="3"
      >
        <b-form-checkbox
          id="logFullClientMessagesCheckbox"
          v-model="currentVersion.logFullClientMessages"
          name="logFullClientMessagesCheckbox"
          value="true"
          unchecked-value="false"
          >Log full client messages</b-form-checkbox
        >
        <b-button type="button" class="btn m-md-1 btn-sm float-right" variant="primary" @click="onSave()"
          >Save</b-button
        >
      </b-form-group>
    </b-container>
  </b-container>
</template>

<script lang="ts">
import { Component, Watch, Vue } from 'vue-property-decorator';
import { IVersion, IProject } from '@/shared';
import { VersionModule } from '@/store/modules/versionmanager';
import { ProjectModule } from '@/store/modules/projectmanager';

@Component({
  name: 'VersionDetails'
})
export default class VersionDetails extends Vue {
  // tslint:disable-next-line
  private projectLabel = '';

  // tslint:disable-next-line
  public projectDisplayText = this.projectLabel;

  get currentVersion(): IVersion {
    return VersionModule.currentVersion;
  }

  set currentVersion(newVersion: IVersion) {
    VersionModule.updateCurrentVersion(newVersion);
  }

  get versionTitle(): string {
    if (this.currentVersion.version) return this.projectDisplayText + ' Version ' + this.currentVersion.version;
    else return '';
  }

  get currentProject(): IProject {
    return ProjectModule.currentProject;
  }

  public changeVersion(version: IVersion) {
    VersionModule.updateCurrentVersion(version);
  }
  public async onSave() {
    await VersionModule.saveCurrentVersion();
  }

  @Watch('currentProject', { immediate: true })
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  public async onProjectChanged(val: IProject, oldVal: IProject) {
    /*
    this.currentVersion = {
      id: undefined,
      projectId: undefined,
      version: undefined,
      description: undefined,
      logFullClientMessages: undefined
    };
    */

    this.projectDisplayText = val?.shortName ? val.shortName : this.projectLabel;
  }
}
</script>
