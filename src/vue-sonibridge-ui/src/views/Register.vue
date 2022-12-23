<template>
  <div class="row h-100 vh-100">
    <div class="col-sm-12 my-auto">
      <SignUp />
    </div>
  </div>
</template>

<script lang="ts">
// @ is an alias to /src
// import ProjectList from '../components/ProjectList.vue';

import SignUp from '@/components/security/SignUp.vue';
import { Component, Vue } from 'vue-property-decorator';

@Component({ name: 'Register', components: { SignUp } })
export default class Register extends Vue {}
</script>
