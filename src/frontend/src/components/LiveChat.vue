<template>
  <div class="chat-container">
    <div class="messages">
      <div v-for="(msg, idx) in messages" :key="idx">
        <strong>{{ msg.user }}:</strong> {{ msg.text }}
      </div>
    </div>
    <form @submit.prevent="sendMessage">
      <input v-model="user" placeholder="User" required />
      <input v-model="message" placeholder="Message" required />
      <button type="submit">Send</button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';

const messages = ref<{ user: string; text: string }[]>([]);
const user = ref('');
const message = ref('');
let connection: signalR.HubConnection | null = null;

onMounted(async () => {
  connection = new signalR.HubConnectionBuilder()
    .withUrl('http://localhost:5005/chatHub') // Adjust port if needed
    .withAutomaticReconnect()
    .build();

  connection.on('ReceiveMessage', (userName: string, msg: string) => {
    messages.value.push({ user: userName, text: msg });
  });

  await connection.start();
});

onUnmounted(() => {
  connection?.stop();
});

async function sendMessage() {
  if (connection && user.value && message.value) {
    await connection.invoke('SendMessage', user.value, message.value);
    message.value = '';
  }
}
</script>

<style scoped>
.chat-container {
  max-width: 400px;
  margin: 2em auto;
  border: 1px solid #ccc;
  padding: 1em;
  border-radius: 8px;
}
.messages {
  min-height: 150px;
  margin-bottom: 1em;
  background: #f9f9f9;
  padding: 0.5em;
  border-radius: 4px;
  overflow-y: auto;
  max-height: 200px;
  color: black;
}
form {
  display: flex;
  gap: 0.5em;
}
input {
  flex: 1;
  padding: 0.5em;
}
button {
  padding: 0.5em 1em;
}
</style>
