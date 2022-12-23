<template>
  <div class="row h-100 vh-100">
    <div class="col-sm-12 my-auto">
      <ConfirmationCode :userName="userName" />
    </div>
  </div>
</template>

<script lang="ts">
import ConfirmationCode from '../components/security/ConfirmationCode.vue';

import { Component, Vue } from 'vue-property-decorator';

@Component({ name: 'ConfirmUser', components: { ConfirmationCode } })
export default class ConfirmUser extends Vue {
  public userName: string | undefined;

  public created(): void {
    this.userName = this.$route.query.userName as string;
  }
}
</script>
