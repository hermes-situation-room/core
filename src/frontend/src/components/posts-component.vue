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
                            This is the posts area.
                        </p>

                        <div class="mt-4">
                            <router-link to="/login" class="btn btn-secondary me-2">
                                Login
                            </router-link>
                            <router-link to="/register" class="btn btn-secondary me-2">
                                Register
                            </router-link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
