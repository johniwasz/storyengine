import store from '@/store';

import { VuexModule, Module, Mutation, Action, getModule } from 'vuex-module-decorators';

import { SET_AUDIOFILES, UPLOAD_AUDIOFILE, DELETE_AUDIOFILE, PLAY_AUDIOFILE } from '@/store/mutation-types';

import {
  IAudioFile,
  IVersion,
  dataService,
  IAudioFileState,
  IVersionAudioFileRequest,
  IVersionAudioFileUploadRequest,
  IPlaybackAudioFile,
  logger
} from '@/shared';

// import store from '@/store';

const EMPTY_PLAYBACKINFO: IPlaybackAudioFile = {
  fileName: '<file not selected>',
  file: new Blob()
};

// @Module({ dynamic: true, name: 'audioFile' })
@Module({ dynamic: true, name: 'audioMod', store })
export class AudioFileManager extends VuexModule implements IAudioFileState {
  public audioFiles: IAudioFile[] = [];
  public playbackInfo: IPlaybackAudioFile = EMPTY_PLAYBACKINFO;

  @Action({ rawError: true })
  public async loadAudioFilesAsync(version: IVersion) {
    if (version?.id && version?.projectId) {
      logger.log(`this.loadAudioFilesAsync(): projectId: ${version.projectId}, versionID: ${version.id}`);
      this.context.commit(SET_AUDIOFILES, await dataService.getAudioFilesAsync(version.projectId, version.id));
    } else {
      throw new Error('version requires id and project id');
    }
  }

  @Action({ rawError: true })
  public async getAudioFileAsync(request: IVersionAudioFileRequest): Promise<Blob> {
    if (request?.versionId && request?.projectId && request?.fileName) {
      const audioBlob: Blob = await dataService.getAudioFileBinaryAsync(
        request.projectId,
        request.versionId,
        request.fileName
      );
      return audioBlob;
    } else {
      throw new Error('getAudioFileAsync requires id,  project id and filename');
    }
  }

  @Action({ rawError: true })
  public async playAudioFileAsync(request: IVersionAudioFileRequest) {
    const audioFile: Blob = await this.getAudioFileAsync(request);

    if (audioFile.size > 0) {
      const playbackInfo: IPlaybackAudioFile = {
        fileName: request.fileName,
        file: audioFile
      };
      this.context.commit(PLAY_AUDIOFILE, playbackInfo);
    }
  }

  @Action({ rawError: true })
  public async uploadAudioFileAsync(request: IVersionAudioFileUploadRequest) {
    if (request?.versionId && request?.projectId && request?.file) {
      const newAudioFile: IAudioFile | null = await dataService.uploadAudioFileAsync(
        request.projectId,
        request.versionId,
        request.file
      );

      if (newAudioFile != null) {
        this.context.commit(UPLOAD_AUDIOFILE, newAudioFile);
      }
    } else {
      throw new Error('uploadAudioFileAsync requires id,  project id and file');
    }
  }

  @Action({ rawError: true })
  public async deleteAudioFileAsync(request: IVersionAudioFileRequest) {
    if (request?.versionId && request?.projectId && request?.fileName) {
      if (await dataService.deleteAudioFileAsync(request.projectId, request.versionId, request.fileName)) {
        this.context.commit(DELETE_AUDIOFILE, request.fileName);
      }
    } else {
      throw new Error('uploadAudioFileAsync requires id,  project id and file');
    }
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_AUDIOFILES](newAudioFiles: IAudioFile[]): void {
    if (newAudioFiles) this.audioFiles = newAudioFiles;
  }

  @Mutation
  // tslint:disable-next-line
  private [UPLOAD_AUDIOFILE](newAudioFile: IAudioFile): void {
    if (newAudioFile != null) {
      let found = false;
      const newAudioFiles: IAudioFile[] = [...this.audioFiles];
      newAudioFiles.forEach((item, i) => {
        if (item.fileName === newAudioFile.fileName) {
          newAudioFiles[i] = newAudioFile;
          found = true;
        }
      }, this);

      if (!found) {
        newAudioFiles.push(newAudioFile);
      }

      this.audioFiles = newAudioFiles;
    }
  }

  @Mutation
  // tslint:disable-next-line
  private [DELETE_AUDIOFILE](deletedFileName: string): void {
    // Nuke the deleted file from the list
    const newAudioFiles = this.audioFiles.filter((item) => {
      return item.fileName !== deletedFileName;
    });

    this.audioFiles = newAudioFiles;

    // If the deleted file is the active playback file, we should nuke
    // that as well.
    if (this.playbackInfo.fileName === deletedFileName) {
      logger.log(`Deleted File: ${deletedFileName} matches plyback file. Clearing playback file.`);
      this.playbackInfo = EMPTY_PLAYBACKINFO;
    }
  }

  @Mutation
  // tslint:disable-next-line
  private [PLAY_AUDIOFILE](playbackInfo: IPlaybackAudioFile): void {
    this.playbackInfo = playbackInfo;
  }
}

export const AudioFileModule = getModule(AudioFileManager);
