import { createRouter, createWebHistory } from "vue-router";
import { watch } from "vue";
import MainPostsComponent from "../components/main-posts-component.vue";
import JournalistPostsComponent from "../components/journalist-posts-component.vue";
import ActivistPostsComponent from "../components/activist-posts-component.vue";
import PostDetailComponent from "../components/post-detail-component.vue";
import EditPostComponent from "../components/edit-post-component.vue";
import LoginComponent from "../components/login-component.vue";
import RegisterComponent from "../components/register-component.vue";
import CreateChatComponent from "../components/create-chat-component.vue";
import ProfileComponent from "../components/profile-component.vue";
import EditProfileComponent from "../components/edit-profile-component.vue";
import { useAuthStore } from "../stores/auth-store";
import ChatsComponent from "../components/chats-component.vue";

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: "/",
            name: "Posts",
            component: MainPostsComponent,
            redirect: "/journalist",
            children: [
                {
                    path: "journalist",
                    name: "Journalist Posts",
                    component: JournalistPostsComponent,
                },
                {
                    path: "activist",
                    name: "Activist Posts",
                    component: ActivistPostsComponent,
                },
            ],
        },
        {
            path: "/post/:id",
            name: "Post Detail",
            component: PostDetailComponent,
        },
        {
            path: "/post/:id/edit",
            name: "Edit Post",
            component: EditPostComponent,
            meta: { requiresAuth: true },
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
            name: "Chats",
            component: ChatsComponent,
            meta: { requiresAuth: true },
        },
        {
            path: "/chat/new",
            name: "Create Chat",
            component: CreateChatComponent,
            meta: { requiresAuth: true },
        },
        {
            path: "/chat/:id",
            name: "Chat Detail",
            component: ChatsComponent,
            meta: { requiresAuth: true },
        },
        {
            path: "/profile",
            name: "Profile",
            component: ProfileComponent,
        },
        {
            path: "/profile/edit",
            name: "EditProfile",
            component: EditProfileComponent,
            meta: { requiresAuth: true },
        },
    ],
});

router.beforeEach(async (to, _, next) => {
    const authStore = useAuthStore();
    
    if (authStore.isLoading.value) {
        await new Promise(resolve => {
            const unwatch = watch(authStore.isLoading, (loading) => {
                if (!loading) {
                    unwatch();
                    resolve(void 0);
                }
            });
        });
    }
    
    const isAuthenticated = authStore.isAuthenticated.value;

    if (to.meta.requiresAuth && !isAuthenticated) {
        next({ name: "Login" });
    } else if (to.meta.guestOnly && isAuthenticated) {
        next({ name: "Posts" });
    } else {
        next();
    }
});

export default router;
