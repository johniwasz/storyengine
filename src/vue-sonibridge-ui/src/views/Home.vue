<template>
  <b-container>This is the home page</b-container>
</template>

<script lang="ts">
// @ is an alias to /src
// import ProjectList from '../components/ProjectList.vue';

import { Component, Vue } from 'vue-property-decorator';

@Component({ name: 'Home' })
export default class Home extends Vue {}
</script>
