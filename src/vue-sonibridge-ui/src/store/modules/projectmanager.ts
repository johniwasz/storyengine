import store from '@/store';

import { VuexModule, Module, Mutation, Action, getModule } from 'vuex-module-decorators';

import {
  // ADD_PROJECT,
  // DELETE_PROJECT,
  // GET_PROJECTS,
  // UPDATE_PROJECT,
  // UPDATE_VERSION,
  SET_PROJECTS,
  SET_CURRENT_PROJECT,
  CLEAR_PROJECTS
} from '@/store/mutation-types';

// https://dev.to/sirtimbly/type-safe-vuex-state-usage-in-components-without-decorators-2b24

import { IProject, dataService, IProjectState } from '@/shared';

@Module({ dynamic: true, name: 'projectMod', store })
export default class ProjectManager extends VuexModule implements IProjectState {
  public currentProject: IProject = { description: undefined, id: undefined, shortName: undefined };
  public projects: IProject[] = [];

  @Action({ rawError: true })
  public async loadProjectsAsync() {
    this.context.commit(SET_PROJECTS, await dataService.getProjectsAsync());
  }

  @Action({ rawError: true })
  public async clearAsync() {
    this.context.commit(CLEAR_PROJECTS);
  }

  @Action({ rawError: true })
  public updateCurrentProject(newProject: IProject) {
    this.context.commit(SET_CURRENT_PROJECT, newProject);
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_PROJECTS](projects: IProject[]): void {
    this.projects = projects;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_CURRENT_PROJECT](newProject: IProject): void {
    this.currentProject = newProject;
  }

  @Mutation
  // tslint:disable-next-line
  private [CLEAR_PROJECTS](): void {
    this.currentProject = { description: undefined, id: undefined, shortName: undefined };
    this.projects = [];
  }
}

export const ProjectModule = getModule(ProjectManager);
