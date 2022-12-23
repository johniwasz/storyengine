<template> <div></div></template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { eventError } from '@/shared/constants';
import { bus } from '@/main.ts';

@Component({ name: 'ErrorToaster' })
export default class ErrorToaster extends Vue {
  protected created() {
    bus.$on(eventError, (data: Error) => {
      this.$bvToast.toast(data.message, { title: 'Error', variant: 'warning' });
    });
  }
}
</script>
