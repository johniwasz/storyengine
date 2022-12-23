<template>
  <div class="row h-100 vh-100">
    <div class="col-sm-12 my-auto">
      <FrontDoor />
    </div>
  </div>
</template>

<script lang="ts">
// @ is an alias to /src
// import ProjectList from '../components/ProjectList.vue';

import FrontDoor from '@/components/security/FrontDoor.vue';
import { Component, Vue } from 'vue-property-decorator';

@Component({ name: 'Login', components: { FrontDoor } })
export default class Login extends Vue {}
</script>
