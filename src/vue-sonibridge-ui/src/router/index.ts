import Vue from 'vue';
import VueRouter from 'vue-router';
// import { authStore } from '@/store';
import { logger } from '@/shared';
// import store from '@/store';
import { AuthModule } from '@/store/modules/authmanager';

Vue.use(VueRouter);

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import(/* webpackChunkName: "about" */ '@/views/Home.vue')
  },
  {
    path: '/about',
    name: 'About',
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import(/* webpackChunkName: "about" */ '@/views/About.vue')
  },

  {
    path: '/version',
    name: 'Version',
    component: () => import(/* webpackChunkName: "version" */ '@/views/Version.vue')
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import(/* webpackChunkName: "login" */ '@/views/Login.vue')
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import(/* webpackChunkName: "register" */ '@/views/Register.vue')
  },
  {
    path: '/confirmuser',
    name: 'ConfirmUser',
    component: () => import(/* webpackChunkName: "confirmuser" */ '@/views/ConfirmUser.vue'),
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    props: (route: any) => ({ query: route.query.q })
  }
];

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
});

router.beforeEach((to, from, next) => {
  // redirect to login page if not logged in and trying to access a restricted page
  const publicPages = ['/login', '/register', '/confirmuser'];
  const authRequired = !publicPages.includes(to.path);
  logger.log('pre-route-check');

  let loggedIn = false;

  AuthModule.init();

  if (AuthModule.status?.isAuthenticated === true) loggedIn = true;

  if (authRequired && !loggedIn) {
    return next('/login');
  }

  next();
});

export default router;
