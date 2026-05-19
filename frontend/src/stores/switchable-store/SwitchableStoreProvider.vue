<template><slot /></template>
<script setup lang="ts">
import { provideSwitchableStore } from "./store";

provideSwitchableStore();
</script>
