<template>
  <b-nav-item-dropdown id="dropdown-right" variant="primary" class="m-2" :text="this.currentUser">
    <b-dropdown-item href="#" @click="signOutAsync()">Sign Out</b-dropdown-item>
  </b-nav-item-dropdown>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

import { AuthModule } from '@/store/modules/authmanager';

import VueRouter from 'vue-router';

@Component({
  name: 'Session'
})
export default class Session extends Vue {
  get currentUser(): string | undefined {
    return AuthModule.userSession?.userName;
  }

  public async signOutAsync(): Promise<void> {
    await AuthModule.signOutAsync();

    const router: VueRouter = this.$router as VueRouter;

    router.push('/login');
  }
}
</script>
