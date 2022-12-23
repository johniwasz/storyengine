import { Store } from 'vuex';
import { getModule } from 'vuex-module-decorators';
import AuthManager from '@/store/modules/authmanager';

let authStore: AuthManager;

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function initializeStores(store: Store<any>): void {
  authStore = getModule(AuthManager, store);
}

export { initializeStores, authStore };
