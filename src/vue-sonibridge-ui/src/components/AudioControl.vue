<template>
  <div>
    <b-container>
      <b-row align-content="center">
        <b-col class="text-right font-weight-bold font-italic">{{ playbackFileName }}</b-col>
        <b-col class="text-left"><audio ref="audioCtrl" controls/></b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script lang="ts">
// @ is an alias to /src
import { Component, Ref, Vue, Watch } from 'vue-property-decorator';
import { AudioFileModule } from '@/store/modules/audiofilemanager';
import { IPlaybackAudioFile, logger } from '@/shared';

@Component({ name: 'AudioControl', components: {} })
export default class AudioControl extends Vue {
  @Ref()
  private readonly audioCtrl!: HTMLAudioElement;

  get PlaybackFile(): IPlaybackAudioFile {
    return AudioFileModule.playbackInfo;
  }

  get playbackFileName(): string {
    return AudioFileModule.playbackInfo.fileName;
  }

  @Watch('PlaybackFile', { immediate: true })
  public async onPlaybackFileChanged(val: IPlaybackAudioFile, oldVal: IPlaybackAudioFile) {
    // If we got passed a file, set the file on the audio control and start playing.
    // If we did not, then clear the file from the control.
    if (val.file.size > 0) {
      this.audioCtrl.src = URL.createObjectURL(val.file);
      this.audioCtrl.play();
    } else if (this.audioCtrl) {
      this.audioCtrl.src = '';
    }

    if (oldVal) logger.log(`old val in audio controls: ${oldVal.fileName}`);
  }
}
</script>
