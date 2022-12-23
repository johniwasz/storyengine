/* eslint-disable @typescript-eslint/camelcase */

// eslint-disable-next-line

import store from '@/store';

import { VuexModule, Module, Action, Mutation, getModule } from 'vuex-module-decorators';

import {
  SET_USERSESSION,
  SET_AUTHERROR,
  SET_LOGINSTATUS,
  SET_ISUSERCONFIRMED,
  SET_USERCREDENTIALS,
  SET_ISAUTHENTICATED,
  SET_ISAUTHINITIALIZED
} from '@/store/mutation-types';

// https://dev.to/sirtimbly/type-safe-vuex-state-usage-in-components-without-decorators-2b24

import {
  IAuthManager,
  IUserCredentials,
  IUserSession,
  IAuthStatus,
  localUserStore,
  ITokenResponse,
  dataService,
  ISignUpRequest,
  ISignOutRequest,
  UserNotConfirmed
} from '@/shared';

// @Module({ dynamic: true, store: store, name: 'auth', namespaced: true, preserveState: true })

@Module({ dynamic: true, name: 'authMod', store, stateFactory: true })
export default class AuthManager extends VuexModule implements IAuthManager {
  public errorMessage = '';

  public userSession: IUserSession = {
    userName: undefined,
    authToken: undefined,
    refreshToken: undefined
  };

  public status: IAuthStatus = {
    isAuthenticated: false,
    loggingIn: false,
    isError: false
  };

  public credentials: IUserCredentials = { name: undefined, password: undefined };

  // assume the user is confirmed unless told otherwise.
  public isConfirmed = true;

  private isInitialized = false;

  @Action({ rawError: true })
  public async autoSignInAsync(): Promise<void> {
    if (this.credentials?.name && this.credentials?.password) {
      await this.signInAsync(this.credentials);
    } else {
      this.context.commit(SET_AUTHERROR, 'Credentials not provided');
    }
  }

  @Action({ rawError: true })
  public init(): void {
    if (!this.isInitialized) {
      console.log('auth is initializing');
      let userSession = localUserStore.getUserStore();

      if (userSession === undefined) {
        userSession = {
          authToken: undefined,
          refreshToken: undefined,
          userName: undefined
        };
      }

      this.context.commit(SET_USERSESSION, userSession);
      if (this.userSession?.authToken) {
        if (this.userSession.authToken.length > 0) this.context.commit(SET_ISAUTHENTICATED, true);
      }
      this.context.commit(SET_ISAUTHINITIALIZED, true);
    }
  }

  @Action({ rawError: true })
  public async signInAsync(creds: IUserCredentials): Promise<void> {
    let signinResults: ITokenResponse | undefined;

    this.context.commit(SET_LOGINSTATUS, true);
    this.context.commit(SET_AUTHERROR, '');
    this.context.commit(SET_USERCREDENTIALS, creds);
    try {
      signinResults = await dataService.signInAsync(creds);

      console.log(signinResults?.permissions);
    } catch (e) {
      if (e instanceof UserNotConfirmed) {
        this.context.commit(SET_ISUSERCONFIRMED, false);
      } else {
        if (e?.message) {
          this.context.commit(SET_AUTHERROR, e.message);
        } else this.context.commit(SET_AUTHERROR, 'unknown error');
      }
    }

    if (signinResults) {
      // const idToken = cogSession.getIdToken();

      const retUser: IUserSession = {
        userName: creds.name,
        authToken: signinResults.authToken,
        refreshToken: signinResults.refreshToken
      };

      this.context.commit(SET_USERSESSION, retUser);
      this.context.commit(SET_ISUSERCONFIRMED, true);
    } else {
      this.context.commit(SET_USERSESSION, undefined);
      this.context.commit(SET_ISUSERCONFIRMED, false);
    }

    this.context.commit(SET_LOGINSTATUS, false);
  }

  @Action({ rawError: true })
  public async signUpAsync(signUpRequest: ISignUpRequest): Promise<void> {
    // this.context.commit(SET_LOGINSTATUS, true);
    this.context.commit(SET_AUTHERROR, '');

    try {
      await dataService.signUpAsync(signUpRequest);
      const userCreds: IUserCredentials = { name: signUpRequest.name, password: signUpRequest.password };
      this.context.commit(SET_USERCREDENTIALS, userCreds);
    } catch (e) {
      if (e?.message) {
        this.context.commit(SET_AUTHERROR, e.message);
      } else this.context.commit(SET_AUTHERROR, 'unknown error');
    }
  }

  @Action({ rawError: true })
  public updateUserName(userName: string | undefined) {
    let creds: IUserCredentials;

    if (this.credentials === undefined) creds = { name: userName, password: undefined };
    else {
      creds = this.credentials;
      creds.name = userName;
    }

    this.context.commit(SET_USERCREDENTIALS, creds);
  }

  @Action({ rawError: true })
  public updatePassword(password: string | undefined) {
    let creds: IUserCredentials;

    console.log(`updating password: ${password}`);
    if (this.credentials === undefined) creds = { name: undefined, password };
    else {
      creds = this.credentials;
      creds.password = password;
    }

    this.context.commit(SET_USERCREDENTIALS, creds);
  }

  @Action({ rawError: true })
  public async signOutAsync(): Promise<void> {
    if (this.userSession) {
      if (this.userSession.authToken) {
        const signOutReq: ISignOutRequest = {
          authToken: this.userSession.authToken
        };

        try {
          await dataService.signOutAsync(signOutReq);
        } catch (e) {
          this.context.commit(SET_AUTHERROR, 'Unexpected error');
        }
      }
    }
    this.context.commit(SET_USERSESSION, undefined);

    this.context.commit(SET_LOGINSTATUS, false);
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_LOGINSTATUS](loginStatus: boolean) {
    this.status.loggingIn = loginStatus;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_ISUSERCONFIRMED](isConfirmed: boolean) {
    this.isConfirmed = isConfirmed;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_USERCREDENTIALS](userCreds: IUserCredentials) {
    this.credentials = userCreds;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_ISAUTHINITIALIZED](isInitialized: boolean) {
    this.isInitialized = isInitialized;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_ISAUTHENTICATED](isAuthenticated: boolean) {
    this.status.isAuthenticated = isAuthenticated;
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_USERSESSION](userSession: IUserSession | undefined): void {
    if (userSession) {
      this.userSession = userSession;
      if (userSession.authToken) {
        // Save the user so that it can be used even if the page is reloaded.
        // User belongs in durable storage, not in memory.
        localUserStore.setUserStore(userSession);
        this.status.isAuthenticated = true;
        this.status.isError = false;
      }
    } else {
      localUserStore.removeUserStore();
      this.status.isAuthenticated = false;
      this.status.isError = false;
      this.userSession = {
        authToken: undefined,
        refreshToken: undefined,
        userName: undefined
      };
    }
  }

  @Mutation
  // tslint:disable-next-line
  private [SET_AUTHERROR](authError: string): void {
    this.errorMessage = authError;
    if (authError && authError.length > 0) {
      this.status.isAuthenticated = false;
      this.status.isError = true;
    } else {
      this.status.isError = false;
    }
  }
}

export const AuthModule = getModule(AuthManager);
