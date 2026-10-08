import { createRouter, createWebHistory } from 'vue-router'
import ProjectsView from '../views/ProjectsView.vue'
import ProjectDetailView from '../views/ProjectDetailView.vue'
import ProjectTasksView from '../views/ProjectTasksView.vue'
import UsersView from '../views/UsersView.vue'
import LoginView from '../views/LoginView.vue'
import NotFoundView from '../views/NotFoundView.vue'
import { useAuthStore } from '../stores/auth'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/projects' },
    { path: '/projects', component: ProjectsView, meta: { title: 'Проекты', requiresAuth: true } },
    { path: '/projects/:id', component: ProjectDetailView, meta: { title: 'Проект', requiresAuth: true } },
    { path: '/projects/:id/tasks', component: ProjectTasksView, meta: { title: 'Задачи', requiresAuth: true } },
    { path: '/users', component: UsersView, meta: { title: 'Пользователи', requiresAuth: true } },
    { path: '/login', component: LoginView, meta: { title: 'Вход', publicOnly: true } },
    { path: '/:pathMatch(.*)*', component: NotFoundView, meta: { title: 'Не найдено', requiresAuth: true } }
  ]
})

router.beforeEach(to => {
  const auth = useAuthStore()
  const authenticated = auth.ensureValidSession()

  if (to.meta.requiresAuth && !authenticated) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  if (to.meta.publicOnly && authenticated) {
    return '/projects'
  }

  return true
})

router.afterEach(to => {
  document.title = `${String(to.meta.title ?? 'ResearchHub')} — ResearchHub`
})
