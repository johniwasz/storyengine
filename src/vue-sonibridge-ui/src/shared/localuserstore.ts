import { IUserSession } from '@/shared';

const localStoreUser = 'user';

export class LocalUserStore {
  public setUserStore(user: IUserSession): void {
    if (user?.authToken) {
      localStorage.setItem(localStoreUser, JSON.stringify(user));
    } else this.removeUserStore();
  }

  public setAuthToken(authToken: string): void {
    let userStore = this.getUserStore();

    if (!userStore) {
      userStore = { refreshToken: undefined, authToken: undefined, userName: undefined };
    }

    userStore.authToken = authToken;

    this.setUserStore(userStore);
  }

  public removeUserStore() {
    localStorage.removeItem(localStoreUser);
  }

  public getAuthToken(): string | undefined {
    const userStore = this.getUserStore();

    if (userStore) return userStore.authToken;

    return undefined;
  }

  public getRefreshToken(): string | undefined {
    const userStore = this.getUserStore();

    if (userStore) return userStore.refreshToken;

    return undefined;
  }

  public getUserStore(): IUserSession | undefined {
    const userText = localStorage.getItem(localStoreUser);

    if (userText) {
      const userSession: IUserSession = JSON.parse(userText) as IUserSession;

      return userSession;
    } else this.removeUserStore();

    return undefined;
  }
}

export const localUserStore: LocalUserStore = new LocalUserStore();
