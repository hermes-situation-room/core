import {createRouter, createWebHistory} from "vue-router";
import MainPostsComponent from "../components/main-posts-component.vue";
import JournalistPostsComponent from "../components/journalist-posts-component.vue";
import ActivistPostsComponent from "../components/activist-posts-component.vue";
import PostDetailComponent from "../components/post-detail-component.vue";
import LoginComponent from "../components/login-component.vue";
import RegisterComponent from "../components/register-component.vue";

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: "/",
            name: "MainPosts",
            component: MainPostsComponent,
            redirect: "/journalist",
            children: [
                {
                    path: "journalist",
                    name: "JournalistPosts",
                    component: JournalistPostsComponent,
                },
                {
                    path: "activist",
                    name: "ActivistPosts",
                    component: ActivistPostsComponent,
                },
            ],
        },
        {
            path: "/post/:id",
            name: "PostDetail",
            component: PostDetailComponent,
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
