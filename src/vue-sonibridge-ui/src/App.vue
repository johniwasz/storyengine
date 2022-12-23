/* eslint-disable no-undef */
<template>
  <main id="app">
    <b-navbar toggleable="sm" type="dark" variant="info" class="bg-dark mb-4" v-if="isAuthenticated">
      <b-collapse id="nav-collapse" is-nav>
        <b-navbar-nav>
          <b-nav-item href="#" to="/">Home</b-nav-item>

          <b-nav-item href="#" to="/version">Version</b-nav-item>

          <b-nav-item href="#" to="/about">About</b-nav-item>
        </b-navbar-nav>
        <b-navbar-nav>
          <ProjectList />

          <VersionList />
        </b-navbar-nav>
        <b-navbar-nav class="ml-auto">
          <Session />
        </b-navbar-nav>
      </b-collapse>
    </b-navbar>

    <main role="main" class="container">
      <router-view />
    </main>
    <ErrorToaster />
    <NotificationController />
  </main>
</template>

<script lang="ts">
import ProjectList from '@/components/ProjectList.vue';
import VersionList from '@/components/VersionList.vue';
import Session from '@/components/Session.vue';
import ErrorToaster from '@/components/ErrorToaster.vue';
import NotificationController from '@/components/NotificationController.vue';
import { AuthModule } from '@/store/modules/authmanager';
// import Acl from 'browser-acl';
import { Vue, Component } from 'vue-property-decorator';
import { FormPlugin } from 'bootstrap-vue';
import { abilitiesPlugin } from '@casl/vue';
import { AppAbility } from '@/shared/appability';

Vue.use(abilitiesPlugin, AppAbility);

Vue.use(FormPlugin);
// For samples on how to define vue components using typescript:
// https://blog.logrocket.com/how-to-write-a-vue-js-app-completely-in-typescript/

@Component({
  components: {
    ProjectList,
    VersionList,
    Session,
    ErrorToaster,
    NotificationController
  }
})
export default class App extends Vue {
  public get isAuthenticated(): boolean {
    return AuthModule.status.isAuthenticated;
  }
}
</script>

<style lang="scss">
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

#nav {
  padding: 30px;

  a {
    font-weight: bold;
    color: #2c3e50;

    &.router-link-exact-active {
      color: #42b983;
    }
  }
}

.nav-active {
  font-weight: bold;
  text-decoration-line: underline !important;
  text-decoration-color: white !important;
  color: white !important;
}
</style>
