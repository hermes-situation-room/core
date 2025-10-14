import {createRouter, createWebHistory} from "vue-router";
import MainPostsComponent from "../components/main-posts-component.vue";
import JournalistPostsComponent from "../components/journalist-posts-component.vue";
import ActivistPostsComponent from "../components/activist-posts-component.vue";
import PostDetailComponent from "../components/post-detail-component.vue";
import LoginComponent from "../components/login-component.vue";
import RegisterComponent from "../components/register-component.vue";
import ChatsListComponent from "../components/chats-list-component.vue";
import ChatDetailComponent from "../components/chat-detail-component.vue";
import CreateChatComponent from "../components/create-chat-component.vue";
import { useAuthStore } from "../stores/auth-store";

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
            meta: { guestOnly: true },
        },
        {
            path: "/register",
            name: "Register",
            component: RegisterComponent,
            meta: { guestOnly: true },
        },
        {
            path: "/chats",
            name: "ChatsList",
            component: ChatsListComponent,
            meta: { requiresAuth: true },
        },
        {
            path: "/chat/new",
            name: "CreateChat",
            component: CreateChatComponent,
            meta: { requiresAuth: true },
        },
        {
            path: "/chat/:id",
            name: "ChatDetail",
            component: ChatDetailComponent,
            meta: { requiresAuth: true },
        },
        {
            path: "/chats",
            name: "ChatsList",
            component: ChatsListComponent,
        },
        {
            path: "/chat/new",
            name: "CreateChat",
            component: CreateChatComponent,
        },
        {
            path: "/chat/:id",
            name: "ChatDetail",
            component: ChatDetailComponent,
        },
    ],
});

router.beforeEach((to, _, next) => {
    const authStore = useAuthStore();
    const isAuthenticated = authStore.isAuthenticated.value;

    if (to.meta.requiresAuth && !isAuthenticated) {
        next({ name: 'Login' });
    } else if (to.meta.guestOnly && isAuthenticated) {
        next({ name: 'MainPosts' });
    } else {
        next();
    }
});

export default router;
