import Vue from 'vue';
import Vuex from 'vuex';
import { IStoreType } from '@/shared';

// import AuthModule from '@/store/modules/authmanager';

Vue.use(Vuex);

export default new Vuex.Store<IStoreType>({});



/*
const store: StoreOptions<IRootState> = {
  state: {
    audioFileState: undefined,
    deploymentState: undefined,
    projectState: undefined,
    versionState: undefined,
    authState: undefined
  },
  modules: {
    AuthModule
  }
};

export default new Vuex.Store<IRootState>(store);
*/
/* import { Store } from 'vuex';
import Vuex from 'vuex';
import Vue from 'vue';
import { initializeStores } from '@/utils/store-accessor';
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const initializer = (store: Store<any>) => initializeStores(store);
export const plugins = [initializer];
export * from '@/utils/store-accessor';

Vue.use(Vuex);

const state = () => ({
  heroes: [],
  villains: [],
});

export default new Vuex.Store({
  strict: process.env.NODE_ENV !== 'production',
  state,
});
*/

/*
import Vue from 'vue';
import Vuex from 'vuex';
import { IRootState } from '../shared';

Vue.use(Vuex);
*./



const store = new Vuex.Store<IRootState>({});
/*
const store: StoreOptions<IRootState> = {
  state: {
    version: '1.0.0' // a simple property
  },
  modules: {
    profile
  }
};
*/

/*
export default store;
*/
