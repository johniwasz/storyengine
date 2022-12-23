import { VuexModule, Module, Mutation, Action, getModule } from 'vuex-module-decorators';
import store from '@/store';
import {
  // ADD_PROJECT,
  // DELETE_PROJECT,
  // GET_PROJECTS,
  // UPDATE_PROJECT,
  // UPDATE_VERSION,
  SET_VERSIONS,
  SET_CURRENT_VERSION
} from '@/store/mutation-types';

import { IVersion, dataService, IVersionState } from '@/shared';
// import store from '@/store';

@Module({ name: 'versionMod', store, dynamic: true })
export default class VersionManager extends VuexModule implements IVersionState {
  public currentVersion: IVersion = {
    id: undefined,
    description: undefined,
    logFullClientMessages: undefined,
    projectId: undefined,
    version: undefined
  };
  public versions: IVersion[] = [];

  @Action({ rawError: true })
  public async loadVersionsAsync(projectId: string) {
    this.context.commit(SET_VERSIONS, await dataService.getVersionsAsync(projectId));
  }

  @Action({ rawError: true })
  public updateCurrentVersion(newVersion: IVersion) {
    this.context.commit(SET_CURRENT_VERSION, newVersion);
  }

  @Action({ rawError: true })
  public async saveCurrentVersion() {
    if (this.currentVersion) {
      this.context.commit(SET_CURRENT_VERSION, await dataService.saveVersion(this.currentVersion));
    }
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_VERSIONS](newVersions: IVersion[]): void {
    if (newVersions) this.versions = newVersions;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_CURRENT_VERSION](newVersion: IVersion): void {
    this.currentVersion = newVersion;
  }
}

export const VersionModule = getModule(VersionManager);
