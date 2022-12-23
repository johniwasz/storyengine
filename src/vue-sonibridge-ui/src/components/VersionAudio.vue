<template>
  <div>
    <h1>Audio Assets</h1>
    <div>
      <b-container>
        <b-row class="text-left">
          <b-col cols="3">
            <b-button v-if="hasCurrentVersion" size="lg" pill variant="primary" @click="showAudioUploadDlg()">
              Upload Audio File
            </b-button>
          </b-col>
          <b-col>
            <AudioControl v-if="hasCurrentVersion" ref="audioCtrl" />
          </b-col>
        </b-row>
      </b-container>
      <UploadFile
        ref="audioFileUploader"
        placeHolder="Choose audio file or drop it here..."
        dropPlaceHolder="Drop audio file here..."
        accept="audio/mpeg"
        uploadTitle="Upload Audio File"
        invalidFileMsg="Please select an audio file"
      />
    </div>
    <div>
      <b-table
        striped
        hover
        sticky-header
        primary-key="fileName"
        sort-by="fileName"
        :items="audioFiles"
        :fields="fields"
      >
        <template v-slot:cell(actions)="row">
          <b-button size="sm" variant="outline-success" @click="playFile(row.item)" class="mr-1">Play</b-button>
          <b-button size="sm" variant="outline-danger" @click="deleteFile(row.item)" class="mr-1">Delete</b-button>
          <b-button size="sm" variant="outline-info" @click="downloadFile(row.item)" class="mr-1">Download</b-button>
        </template>
      </b-table>
    </div>
    <b-modal
      id="confirmDeleteModal"
      ref="confirmDeleteModal"
      size="sm"
      title="Delete Audio File"
      @ok="handleDelete"
      footer-bg-variant="warning"
      button-size="sm"
      ok-title="Delete"
      centered
    >
      <div class="d-block text-center">Delete file {{ fileToDelete.fileName }}?</div>
    </b-modal>
  </div>
</template>

<script lang="ts">
// @ is an alias to /src
import { format, parseISO } from 'date-fns';
import { inputDateFormat } from '@/shared/constants';
import { VersionModule } from '@/store/modules/versionmanager';
import { AudioFileModule } from '@/store/modules/audiofilemanager';
import {
  IVersion,
  IAudioFile,
  IDisplayAudioFile,
  IVersionAudioFileRequest,
  IVersionAudioFileUploadRequest,
  IAudioFileTableField,
  logger
} from '@/shared';
import AudioControl from './AudioControl.vue';
import UploadFile from './UploadFile.vue';
import { Component, Vue, Ref, Watch } from 'vue-property-decorator';
import { BModal } from 'bootstrap-vue';

@Component({ name: 'VersionAudio', components: { AudioControl, UploadFile } })
export default class VersionAudio extends Vue {
  @Ref()
  private readonly audioFileUploader!: UploadFile;

  @Ref()
  private readonly confirmDeleteModal!: BModal;

  private fileToDeleteInternal: IDisplayAudioFile = {
    fileName: '',
    size: 0,
    lastModified: undefined
  };

  get currentVersion(): IVersion {
    return VersionModule.currentVersion;
  }

  get audioFiles(): IDisplayAudioFile[] {
    return AudioFileModule.audioFiles.map((audioFile) => {
      return this.convertForDisplay(audioFile);
    });
  }

  get fields(): IAudioFileTableField[] {
    return [
      { key: 'fileName', label: 'Filename' },
      { key: 'size', label: 'Size' },
      { key: 'lastModified', label: 'Last Modified' },
      { key: 'actions', label: 'Actions' }
    ];
  }

  get hasCurrentVersion(): boolean {
    return this.currentVersion.id !== undefined;
  }

  get fileToDelete(): IDisplayAudioFile {
    if (this.fileToDeleteInternal.fileName !== '') {
      return this.fileToDeleteInternal;
    }

    return {
      fileName: '',
      size: 0,
      lastModified: undefined
    };
  }

  public async playFile(item: IDisplayAudioFile) {
    if (this.hasCurrentVersion) {
      const currVersion: IVersion = this.currentVersion;
      const request: IVersionAudioFileRequest = {
        projectId: currVersion.projectId,
        versionId: currVersion.id,
        fileName: item.fileName
      };
      await AudioFileModule.playAudioFileAsync(request);
    }
  }

  public async handleDelete() {
    if (this.hasCurrentVersion && this.fileToDelete.fileName !== '') {
      const currVersion: IVersion = this.currentVersion;
      const request: IVersionAudioFileRequest = {
        projectId: currVersion.projectId,
        versionId: currVersion.id,
        fileName: this.fileToDelete.fileName
      };
      await AudioFileModule.deleteAudioFileAsync(request);
    }
  }

  public async deleteFile(item: IDisplayAudioFile) {
    this.fileToDeleteInternal = item;
    this.confirmDeleteModal.show();
  }

  public async downloadFile(item: IDisplayAudioFile) {
    if (this.hasCurrentVersion) {
      const currVersion: IVersion = this.currentVersion;
      const request: IVersionAudioFileRequest = {
        projectId: currVersion.projectId,
        versionId: currVersion.id,
        fileName: item.fileName
      };

      // Download the file using our platform agnostic API then commit it to disk
      // by creating an anchor tag, setting the href and the filename and then clicking
      // the tag
      const audioFile: Blob = await AudioFileModule.getAudioFileAsync(request);
      if (audioFile.size > 0) {
        const fileLink: HTMLAnchorElement = document.createElement('a');
        fileLink.href = window.URL.createObjectURL(audioFile);
        fileLink.download = item.fileName;
        fileLink.click();

        // cleanup the URL for safety
        URL.revokeObjectURL(fileLink.href);
      }
    }
  }

  public showAudioUploadDlg() {
    this.audioFileUploader.show();
  }

  @Watch('currentVersion', { immediate: true })
  public async onVersionChanged(val: IVersion, oldVal: IVersion) {
    if (val.version) {
      AudioFileModule.audioFiles.splice(0, AudioFileModule.audioFiles.length);
      logger.log(`Version Audio: project id: ${val.projectId}, version id: ${val.id}`);
      await AudioFileModule.loadAudioFilesAsync(val);
    } else logger.log('no new value?');

    if (oldVal) logger.log(`old val in version audio: ${oldVal.version}`);
  }

  protected mounted() {
    this.audioFileUploader.uploadFile = async (fileParam: File) => {
      const currVersion: IVersion = this.currentVersion;
      if (currVersion) {
        const request: IVersionAudioFileUploadRequest = {
          projectId: currVersion.projectId,
          versionId: currVersion.id,
          file: fileParam
        };
        await AudioFileModule.uploadAudioFileAsync(request);
      }
    };
  }

  private convertForDisplay(audioFile: IAudioFile): IDisplayAudioFile {
    const newAudioFile: IDisplayAudioFile = {
      fileName: audioFile.fileName,
      size: audioFile.size,
      lastModified: this.formatPublishDate(audioFile.lastModified)
    };
    return newAudioFile;
  }

  private formatPublishDate(publishDate: Date | undefined): string | undefined {
    if (publishDate) return format(parseISO(publishDate.toString()), inputDateFormat);

    return '';
  }
}
</script>
