<script setup lang="ts">
</script>

<template>
    <div class="container-fluid py-4">
        <div class="row justify-content-center">
            <div class="col-md-8">
                <div class="card">
                    <div class="card-body text-center">
                        <h3 class="card-title">Posts Content</h3>
                        <p class="card-text">
                            This is the posts area. The navbar above is always visible
                            and will remain on top of all other content across all routes.
                        </p>

            <div class="mt-4">
                <router-link to="/login" class="btn btn-primary me-2">
                    Login
                </router-link>
                <router-link to="/register" class="btn btn-success me-2">
                    Register
                </router-link>
                <button class="btn btn-secondary">
                    Posts Action
                </button>
            </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
