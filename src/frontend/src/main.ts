import { createApp } from "vue";
import router from "./router";
import App from "./App.vue";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap/dist/js/bootstrap.bundle.min.js";
import "@mdi/font/css/materialdesignicons.css";
import "@fortawesome/fontawesome-free/css/all.min.css";

import "@fontsource/roboto/100.css";
import "@fontsource/roboto/300.css";
import "@fontsource/roboto/400.css";
import "@fontsource/roboto/500.css";
import "@fontsource/roboto/700.css";
import "@fontsource/roboto/900.css";

import "@fontsource/roboto/100-italic.css";
import "@fontsource/roboto/300-italic.css";
import "@fontsource/roboto/400-italic.css";
import "@fontsource/roboto/500-italic.css";
import "@fontsource/roboto/700-italic.css";
import "@fontsource/roboto/900-italic.css";

import { useAuthStore } from "./stores/auth-store";

import { registerSW } from "virtual:pwa-register";

const app = createApp(App);
app.use(router);

const authStore = useAuthStore();
authStore.initAuth().then(() => {
    app.mount("#app");
});

registerSW({ immediate: true });
