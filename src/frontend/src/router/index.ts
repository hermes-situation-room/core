import {createRouter, createWebHistory} from "vue-router";
import PostsComponent from "../components/posts-component.vue";
import LoginComponent from "../components/login-component.vue";
import RegisterComponent from "../components/register-component.vue";

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: "/",
            name: "Posts",
            component: PostsComponent,
        },
        {
            path: "/login",
            name: "Login",
            component: LoginComponent,
        },
        {
            path: "/register",
            name: "Register",
            component: RegisterComponent,
        },
    ],
});

export default router;
