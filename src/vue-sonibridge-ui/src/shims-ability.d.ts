import { AppAbility } from '@/shared/appability';

declare module 'vue/types/vue' {
  interface Vue {
    $ability: AppAbility;
    $can(this: this, ...args: Parameters<this['$ability']['can']>): boolean;
  }
}
declare module 'vue/types/options' {
  interface ComponentOptions<V extends Vue> {
    ability?: AppAbility;
  }
}
