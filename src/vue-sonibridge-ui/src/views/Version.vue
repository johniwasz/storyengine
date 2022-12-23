<template>
  <div class="home">
    <!--    
    <div class="container">
      <VersionDefault />
    </div>
    <sidebar-menu :menu="menu">
          <span slot="toggle-icon">toggle-icon</span>
    </sidebar-menu>
    -->

    <b-container fluid>
      <b-row>
        <b-button ref="sidebarToggle" v-b-toggle.sidebar-variant>{{ menuText }}</b-button>
        <b-sidebar id="sidebar-variant" title="Version" bg-variant="dark" text-variant="light" shadow>
          <template v-slot:default="{ hide }">
            <div class="p-3">
              <b-button :variant="isActiveButtonStyle('info')" block @click="clickButton('info')">Info</b-button>
              <b-button :variant="isActiveButtonStyle('audio')" block @click="clickButton('audio')">Audio</b-button>
              <b-button :variant="isActiveButtonStyle('images')" block @click="clickButton('images')">Images</b-button>
              <b-button variant="primary" block @click="hide">Close</b-button>
            </div>
          </template>
        </b-sidebar>
      </b-row>
      <VersionDefault v-if="activeControl == 'info'" />
      <VersionAudio v-if="activeControl == 'audio'" />
      <VersionImages v-if="activeControl == 'images'" />
    </b-container>
  </div>
</template>

<script lang="ts">
// @ is an alias to /src
import VersionDefault from '@/components/VersionDefault.vue';
import VersionAudio from '@/components/VersionAudio.vue';
import VersionImages from '@/components/VersionImages.vue';
import { Component, Ref, Vue } from 'vue-property-decorator';
import { IVersion } from '@/shared';
import { VersionModule } from '@/store/modules/versionmanager';
import { BButton } from 'bootstrap-vue';

@Component({ name: 'Version', components: { VersionDefault, VersionAudio, VersionImages } })
export default class Version extends Vue {
  private menuText = 'Operations';
  private activeCtl = 'info';

  // Allows us to reference the sidebarToggle control to manually make the sidebar appear
  @Ref()
  private readonly sidebarToggle!: BButton;

  public clickButton(view: string) {
    this.activeCtl = view;
    this.sidebarToggle.click();
  }

  public isActiveButtonStyle(view: string): string {
    return this.isActive(view) ? 'success' : 'primary';
  }

  public isActive(view: string): boolean {
    return this.activeControl === view;
  }

  get activeControl() {
    return this.activeCtl;
  }

  get menuButtonText() {
    return this.menuText;
  }

  set menuButtonText(s: string) {
    this.menuText = s;
  }

  get currentVersion(): IVersion {
    return VersionModule.currentVersion;
  }
}
</script>
