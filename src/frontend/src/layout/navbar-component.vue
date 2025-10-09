<script setup lang="ts">
    import {onMounted, ref, watch} from 'vue'
    import faviconUrl from '../assets/logo_situation_room_URL.png'
    import logoUrl from '../assets/logo_situation_room.png'
    import {RouterLink, useRoute} from 'vue-router'

    const route = useRoute()
    const pageTitle = ref('Posts')

    watch(() => route.name, (newRouteName) => {
        pageTitle.value = newRouteName as string || 'Posts'
    }, {immediate: true})

    onMounted(() => {
        const existing = document.querySelector<HTMLLinkElement>('link[rel="icon"]')
        const link = existing ?? document.createElement('link')
        link.rel = 'icon'
        link.type = 'image/png'
        link.href = faviconUrl
        if (!existing) document.head.appendChild(link)
    })

    function scaleUp(e: MouseEvent) {
        (e.currentTarget as HTMLElement).style.transform = 'scale(1.15)'
    }

    function scaleDown(e: MouseEvent) {
        (e.currentTarget as HTMLElement).style.transform = 'scale(1)'
    }
</script>

<template>
    <div class="sticky-top" style="background-color: #F5F5F5;">
        <nav class="navbar py-2 px-3" style="background-color: #F5F5F5;">
            <div class="container-fluid d-flex justify-content-between align-items-center flex-nowrap">
                <RouterLink to="/" class="navbar-brand d-flex align-items-center">
                    <img
                        :src="logoUrl"
                        alt="Situation Room logo"
                        class="logo"
                        style="height: clamp(1.8rem, 4vw, 3rem); width: auto; transition: transform 0.2s ease;"
                    />
                </RouterLink>

                <div
                    class="flex-grow-1 text-center"
                    style="font-size: clamp(1rem, 2vw, 1.5rem); font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;"
                >
                    {{ pageTitle }}
                </div>

                <div class="d-flex align-items-center">
                    <RouterLink to="/login">
                        <v-icon
                            class="icon"
                            style="font-size: clamp(1.25rem, 3vw, 2rem); cursor: pointer; transition: transform 0.2s ease; color: #000;"
                            @mouseover="scaleUp"
                            @mouseout="scaleDown"
                        >mdi-account-search
                        </v-icon>
                    </RouterLink>

                    <RouterLink to="/login" style="margin-left: clamp(0.5rem, 2vw, 2rem);">
                        <v-icon
                            class="icon"
                            style="font-size: clamp(1.25rem, 3vw, 2rem); cursor: pointer; transition: transform 0.2s ease; color: #000;"
                            @mouseover="scaleUp"
                            @mouseout="scaleDown"
                        >mdi-message-processing-outline
                        </v-icon>
                    </RouterLink>

                    <RouterLink to="/login" style="margin-left: clamp(0.5rem, 2vw, 2rem);">
                        <v-icon
                            class="icon"
                            style="font-size: clamp(1.25rem, 3vw, 2rem); cursor: pointer; transition: transform 0.2s ease; color: #000;"
                            @mouseover="scaleUp"
                            @mouseout="scaleDown"
                        >mdi-account-circle
                        </v-icon>
                    </RouterLink>
                </div>
            </div>
        </nav>
        <hr style="border: none; height: 1px; background-color: #000; margin: 0;"/>
    </div>
</template>
