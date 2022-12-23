<template>
  <b-nav-item-dropdown
    id="lstProjects"
    name="lstProjects"
    v-model="this.currentProject"
    variant="primary"
    class="m-md-2"
    v-bind:text="this.projectDisplayText"
  >
    <b-dropdown-item disabled value="0">{{ this.projectLabel }}</b-dropdown-item>
    <b-dropdown-item
      v-for="project in this.projects"
      :key="project.id"
      :value="project.shortName"
      @click="changeProject(project)"
      >{{ project.shortName }}</b-dropdown-item
    >
  </b-nav-item-dropdown>
</template>

<script lang="ts">
import { Component, Watch, Vue } from 'vue-property-decorator';
import { IProject, IUserSession } from '@/shared';
import { ProjectModule } from '@/store/modules/projectmanager';
import { AuthModule } from '@/store/modules/authmanager';

@Component({
  name: 'ProjectList'
})
export default class ProjectList extends Vue {
  // tslint:disable-next-line
  private projectLabel = 'Select a Project';

  // tslint:disable-next-line
  public projectDisplayText = this.projectLabel;

  get projects(): IProject[] {
    return ProjectModule.projects;
  }

  get currentProject(): IProject {
    return ProjectModule.currentProject;
  }

  set currentProject(project: IProject) {
    ProjectModule.updateCurrentProject(project);
  }

  get currentSession(): IUserSession | undefined {
    return AuthModule.userSession;
  }

  public changeProject(project: IProject) {
    this.currentProject = project;
    this.projectDisplayText = ProjectModule.currentProject?.shortName
      ? ProjectModule.currentProject.shortName
      : this.projectLabel;
  }

  @Watch('currentSession', { immediate: true })
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  public async onSessionChanged(val: IUserSession, oldVal: IUserSession) {
    if (AuthModule.status.isAuthenticated) await ProjectModule.loadProjectsAsync();
    else await ProjectModule.clearAsync();
  }

  protected async created() {
    // this.currentProject = new Project();
    if (AuthModule.status.isAuthenticated) await ProjectModule.loadProjectsAsync();
  }
}
</script>
