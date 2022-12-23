import { VuexModule, Module, Action, getModule } from 'vuex-module-decorators';

import store from '@/store';

import {
  dataService,
  IUserConfirmationRequest,
  IUserConfirmationResponse,
  INewUserConfirmationRequest
} from '@/shared';

@Module({ name: 'userMod', store, dynamic: true })
export default class UserConfirmationManager extends VuexModule {
  @Action({ rawError: true })
  public async confirmUserAsync(confirmRequest: IUserConfirmationRequest): Promise<IUserConfirmationResponse> {
    return dataService.confirmUserAsync(confirmRequest);
  }

  @Action({ rawError: true })
  public async requestNewCode(newCodeRequest: INewUserConfirmationRequest): Promise<void> {
    return dataService.requestNewUserConfirmationCode(newCodeRequest);
  }
}

export const UserConfirmationModule = getModule(UserConfirmationManager);
