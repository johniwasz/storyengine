<template>
  <div>
    <b-modal id="uploadAudioModal" ref="uploadAudioModal" :title="uploadTitle" @ok="handleOk">
      <b-form @submit.stop.prevent>
        <b-form-file
          v-model="file"
          :placeholder="placeHolder"
          :drop-placeholder="dropPlaceHolder"
          :accept="accept"
          required
          size="sm"
          :state="isFileValid()"
        />
        <b-form-invalid-feedback :state="isFileValid()">
          {{ this.invalidFileMsg }}
        </b-form-invalid-feedback>
      </b-form>
    </b-modal>
  </div>
</template>

<script lang="ts">
// @ is an alias to /src
import { Component, Ref, Prop, Vue } from 'vue-property-decorator';
import { VersionModule } from '@/store/modules/versionmanager';
import { IVersion } from '@/shared';
import { BModal, BvEvent } from 'bootstrap-vue';

@Component({ name: 'UploadFile', components: {} })
export default class UploadFile extends Vue {
  @Prop({ default: 'Choose a file or drop it here...' })
  public placeHolder: string | undefined;

  @Prop({ default: 'Drop file here...' })
  public dropPlaceHolder: string | undefined;

  @Prop({ default: null })
  public accept: string | undefined;

  @Prop({ default: 'Upload File' })
  public uploadTitle: string | undefined;

  @Prop({ default: 'Please select a filebar' })
  public invalidFileMsg: string | undefined;

  @Ref()
  private readonly uploadAudioModal!: BModal;

  private currentFile: File | null = null;

  // Callers can reset this value to an appropriate function.
  public uploadFile = async (file: File) => {
    alert('Uploading file: ' + file.name);
  };

  get currentVersion(): IVersion {
    return VersionModule.currentVersion;
  }

  get file(): File | null {
    return this.currentFile;
  }

  set file(f: File | null) {
    this.currentFile = f;
  }

  public handleOk(bvModalEvt: BvEvent): void {
    // Prevent modal from closing
    bvModalEvt.preventDefault();
    // Trigger submit handler
    this.handleSubmit();
  }

  public async handleSubmit(): Promise<void> {
    // Exit when the form isn't valid
    if (!this.checkFormValidity()) {
      return;
    } else if (this.file != null) {
      await this.uploadFile(this.file);
    }

    // Hide the modal manually
    this.$nextTick(() => {
      this.uploadAudioModal.hide();
    });
  }

  public show() {
    this.file = null;
    this.uploadAudioModal.show();
  }

  public isFileValid(): boolean {
    return this.file != null && (this.accept == null || this.file.type === this.accept);
  }

  private checkFormValidity(): boolean {
    return this.isFileValid();
  }
}
</script>
