import { createRouter, createWebHistory } from "vue-router";
import { watch } from "vue";
import PostsPage from "../pages/PostsPage.vue";
import JournalistPostsPage from "../pages/JournalistPostsPage.vue";
import ActivistPostsPage from "../pages/ActivistPostsPage.vue";
import PostDetailPage from "../pages/PostDetailPage.vue";
import EditPostPage from "../pages/EditPostPage.vue";
import LoginPage from "../pages/LoginPage.vue";
import RegisterPage from "../pages/RegisterPage.vue";
import CreateChatPage from "../pages/CreateChatPage.vue";
import ProfilePage from "../pages/ProfilePage.vue";
import EditProfilePage from "../pages/EditProfilePage.vue";
import ChatsPage from "../pages/ChatsPage.vue";
import { useAuthStore } from "../stores/auth-store";

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: "/",
            name: "Posts",
            component: PostsPage,
            redirect: "/journalist",
            children: [
                {
                    path: "journalist",
                    name: "Journalist Posts",
                    component: JournalistPostsPage,
                },
                {
                    path: "activist",
                    name: "Activist Posts",
                    component: ActivistPostsPage,
                },
            ],
        },
        {
            path: "/post/:id",
            name: "Post Detail",
            component: PostDetailPage,
        },
        {
            path: "/post/:id/edit",
            name: "Edit Post",
            component: EditPostPage,
            meta: { requiresAuth: true },
        },
        {
            path: "/login",
            name: "Login",
            component: LoginPage,
            meta: { guestOnly: true },
        },
        {
            path: "/register",
            name: "Register",
            component: RegisterPage,
            meta: { guestOnly: true },
        },
        {
            path: "/chats",
            name: "Chats",
            component: ChatsPage,
            meta: { requiresAuth: true },
        },
        {
            path: "/chat/new",
            name: "Create Chat",
            component: CreateChatPage,
            meta: { requiresAuth: true },
        },
        {
            path: "/chat/:id",
            name: "Chat Detail",
            component: ChatsPage,
            meta: { requiresAuth: true },
        },
        {
            path: "/profile",
            name: "Profile",
            component: ProfilePage,
        },
        {
            path: "/profile/edit",
            name: "EditProfile",
            component: EditProfilePage,
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
