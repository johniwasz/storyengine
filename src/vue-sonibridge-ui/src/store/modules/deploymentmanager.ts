import { VuexModule, Module, Mutation, Action, getModule } from 'vuex-module-decorators';
import store from '@/store';
import { SET_DEPLOYMENTS, SET_CURRENT_DEPLOYMENT, ADD_DEPLOYMENT, DELETE_DEPLOYMENT } from '@/store/mutation-types';

import {
  dataService,
  IDeploymentState,
  IDeployment,
  IVersion,
  IAddDeploymentRequest,
  IDeleteDeploymentRequest
} from '@/shared';
// import store from '@/store';

@Module({ dynamic: true, name: 'deploymentMod', store })
class DeploymentManager extends VuexModule implements IDeploymentState {
  public currentDeployment: IDeployment = {
    id: undefined,
    versionId: undefined,
    clientType: 'unknown',
    alias: undefined,
    clientId: undefined,
    projectId: undefined,
    publishDate: undefined
  };
  public deployments: IDeployment[] = [];

  @Action({ rawError: true })
  public async loadDeploymentsAsync(parentVersion: IVersion) {
    if (parentVersion?.projectId && parentVersion?.id) {
      this.context.commit(
        SET_DEPLOYMENTS,
        await dataService.getDeploymentsAsync(parentVersion.projectId, parentVersion.id)
      );
    } else {
      throw new Error('parentVersion must include both a projectId and id');
    }
  }

  @Action({ rawError: true })
  public updateCurrentDeployment(newDeployment: IDeployment) {
    this.context.commit(SET_CURRENT_DEPLOYMENT, newDeployment);
  }

  @Action({ rawError: true })
  public async addDeploymentAsync(newDeployment: IAddDeploymentRequest) {
    if (newDeployment) {
      this.context.commit(ADD_DEPLOYMENT, await dataService.addDeploymentAsync(newDeployment));
    }
  }

  @Action({ rawError: true })
  public async deleteDeploymentAsync(deleteRequest: IDeleteDeploymentRequest) {
    await dataService.deleteDeploymentAsync(
      deleteRequest.projectId,
      deleteRequest.versionId,
      deleteRequest.deploymentId
    );

    this.context.commit(DELETE_DEPLOYMENT, deleteRequest.deploymentId);
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_DEPLOYMENTS](newDeployments: IDeployment[]): void {
    if (newDeployments) this.deployments = newDeployments;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_CURRENT_DEPLOYMENT](newDeployment: IDeployment): void {
    this.currentDeployment = newDeployment;
  }

  @Mutation
  // tslint:disable-next-line
  private [ADD_DEPLOYMENT](newDeployment: IDeployment): void {
    if (newDeployment.id) this.deployments.push(newDeployment);
  }

  @Mutation
  // tslint:disable-next-line
  private [DELETE_DEPLOYMENT](deploymentId: string): void {
    const index = this.deployments.findIndex((x) => deploymentId === x.id);
    this.deployments.splice(index, 1);
  }
}

export const DeploymentModule = getModule(DeploymentManager);
