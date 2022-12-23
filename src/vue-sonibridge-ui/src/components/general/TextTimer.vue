<template>
  <div>{{ intervalCount }}</div>
</template>

<script lang="ts">
import { Component, Prop, Vue, Emit } from 'vue-property-decorator';
import { logger } from '@/shared';

@Component({
  name: 'sb-timer-simple',
  inheritAttrs: false
})
export default class TextTimer extends Vue {
  @Prop({ default: 10 })
  public countDown!: number;

  protected timer!: TimerHandler;

  protected intervalCount = this.countDown;

  public startCountdown() {
    this.countdownTimer();
  }

  @Emit('countdownDone')
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  public countdownDone() {
    logger.log('countdown done');
  }

  protected countdownTimer() {
    this.intervalCount = this.countDown;
    const timerId = setInterval(() => {
      this.intervalCount--;
    }, 1000);

    setTimeout(() => {
      clearInterval(timerId);
      this.countdownDone();
    }, this.countDown * 1000);
    /*
    if (this.internalCountDown > 0) {
      this.timer = setTimeout(() => {
        this.internalCountDown -= 1;
        this.countdownTimer();
      }, 1000);

      setTimeout(
        this.timer,
        () => {
          this.internalCountDown -= 1;
          this.countdownTimer();
        },
        this.countDown * 1000
      );
    }
    */
  }
}
</script>
