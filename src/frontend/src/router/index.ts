import { createRouter, createWebHistory } from "vue-router";
import HermesPostsComponent from "../components/hermes-posts-component.vue";
import HermesLoginComponent from "../components/hermes-login-component.vue";
import HermesRegisterComponent from "../components/hermes-register-component.vue";

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: "/",
            name: "Posts",
            component: HermesPostsComponent,
        },
        {
            path: "/login",
            name: "Login",
            component: HermesLoginComponent,
        },
        {
            path: "/register",
            name: "Register",
            component: HermesRegisterComponent,
        },
    ],
});

export default router;
