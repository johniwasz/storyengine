import Vue from 'vue';
// Vue.config.devtools = process.env.NODE_ENV !== 'production';
Vue.config.devtools = true;
import { BootstrapVue, IconsPlugin, CollapsePlugin } from 'bootstrap-vue';
import App from '@/App.vue';
import '@/registerServiceWorker';
import router from '@/router';
import store from '@/store';
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import { eventError, logger } from '@/shared';
import { DirectiveBinding } from 'vue/types/options';
// import Vuex from 'vuex';
// import { plugins } from '@/shared';

// Refer to this template for canonical examples of using typescript and Vue:
// https://github.com/Armour/vue-typescript-admin-template

// Reference article for integrating Vue with TypeScript
// https://blog.logrocket.com/how-to-write-a-vue-js-app-completely-in-typescript/

Vue.config.productionTip = false;

export const bus = new Vue();

Vue.directive('visible', (el: HTMLElement, binding: DirectiveBinding) => {
  el.style.visibility = binding.value ? 'visible' : 'hidden';
});

// eslint-disable-next-line @typescript-eslint/no-explicit-any
Vue.prototype.$sync = function(key: string, value: any) {
  this.$emit(`update:${key}`, value);
};

Vue.use(BootstrapVue);
Vue.use(IconsPlugin);
Vue.use(CollapsePlugin);

Vue.config.errorHandler = (err, vm, info) => {
  // err: error trace
  // vm: component in which error occured
  // info: Vue specific error information such as lifecycle hooks, events etc.
  // TODO: Perform any custom logic or log to server

  if (err) logger.info('Unexpected error: ', err.message);
  if (vm) logger.info('Current view model', vm);

  if (info) logger.log(info);

  bus.$emit(eventError, err);
};

new Vue({
  router,
  store,
  render: (h) => h(App)
}).$mount('#app');
