<template>
    
      <form class="container" @submit.prevent="emit('submit')">
        <slot name="formFields"></slot>     
        <menu>
          <slot name="formMenuListItems"></slot>  
        </menu>
      </form>

  </template>
  
  <script setup lang="ts"> 

  
  const emit = defineEmits<{
    (e: 'submit'): void 
  }>()  

  </script>
  
  <style scoped lang="scss">


      menu {
          margin-top: var(--spacing-large);
          display: flex;
          gap: var(--spacing-default);
          justify-content: flex-end;
      }

      .container {
          display: flex;
          flex-direction: column;
          gap: var(--spacing-default);
      }

      form {
          margin-top: var(--spacing-default);
      }

      label > span {
          display: block;
      }
  </style>
  