import type { Term } from './glossary'

// DevOps / cloud / container terminology, plain-English.
export const GLOSSARY_DEVOPS: Term[] = [
  // Containers
  { term: 'Container', category: 'Containers', definition: 'A lightweight, isolated package of an app + its dependencies that runs the same everywhere. Shares the host OS kernel.' },
  { term: 'Image', category: 'Containers', definition: 'The immutable blueprint a container is created from (layers of filesystem + config).' },
  { term: 'Dockerfile', category: 'Containers', definition: 'A recipe describing how to build an image, step by step.', example: 'FROM …\nCOPY …\nRUN …' },
  { term: 'Registry', category: 'Containers', definition: 'Where images are stored and shared (Docker Hub, GHCR, ACR, ECR).' },
  { term: 'Volume', category: 'Containers', definition: 'Persistent storage that outlives a container (so data isn\'t lost on restart).' },
  { term: 'docker-compose', category: 'Containers', definition: 'Defines and runs multiple containers together locally with one command.' },

  // Orchestration
  { term: 'Kubernetes (K8s)', category: 'Orchestration', definition: 'A container orchestrator: schedules, scales, heals, and networks containers across machines.' },
  { term: 'Pod', category: 'Orchestration', definition: 'The smallest deployable unit in K8s — one or more containers sharing a network.' },
  { term: 'Deployment', category: 'Orchestration', definition: 'Declares the desired number of pod replicas and manages rollouts/rollbacks.' },
  { term: 'Service', category: 'Orchestration', definition: 'A stable address that load-balances traffic to matching pods.' },
  { term: 'Ingress', category: 'Orchestration', definition: 'Routes external HTTP(S) traffic into services (host/path rules, TLS).' },
  { term: 'Replica', category: 'Orchestration', definition: 'One running copy of a pod; more replicas = more capacity + resilience.' },
  { term: 'Namespace', category: 'Orchestration', definition: 'A virtual cluster partition for isolating and organizing resources.' },

  // CI/CD
  { term: 'CI (Continuous Integration)', category: 'CI/CD', definition: 'Automatically build and test every change so problems surface early.' },
  { term: 'CD (Continuous Delivery/Deployment)', category: 'CI/CD', definition: 'Automatically release changes to staging/production.' },
  { term: 'Pipeline', category: 'CI/CD', definition: 'The automated sequence of build → test → deploy steps.' },
  { term: 'Artifact', category: 'CI/CD', definition: 'A build output (binary, image, package) passed between pipeline stages.' },
  { term: 'Runner / Agent', category: 'CI/CD', definition: 'The machine that executes pipeline jobs.' },

  // Delivery strategies
  { term: 'Blue-green deployment', category: 'Delivery', definition: 'Two environments; switch all traffic from old to new at once (instant rollback).' },
  { term: 'Canary deployment', category: 'Delivery', definition: 'Release to a small % of users first, watch metrics, then ramp up.' },
  { term: 'Rollback', category: 'Delivery', definition: 'Revert to the previous known-good version when a release goes wrong.' },
  { term: 'Health check (liveness/readiness)', category: 'Delivery', definition: 'Liveness = restart if unhealthy; readiness = withhold traffic until ready.' },

  // Infrastructure & ops
  { term: 'IaC (Infrastructure as Code)', category: 'Infrastructure', definition: 'Define infrastructure in version-controlled files (Terraform, Bicep) instead of clicking a portal.' },
  { term: 'Load balancer', category: 'Infrastructure', definition: 'Spreads incoming requests across many servers/pods.' },
  { term: 'Reverse proxy', category: 'Infrastructure', definition: 'A front server (Nginx, YARP) that forwards requests to backends, doing TLS, caching, routing.' },
  { term: 'Horizontal vs vertical scaling', category: 'Infrastructure', definition: 'Horizontal = more machines; vertical = a bigger machine. Cloud favors horizontal.' },
  { term: 'Observability', category: 'Infrastructure', definition: 'Logs, metrics, and traces that let you understand a system in production.' },
  { term: 'Serverless', category: 'Infrastructure', definition: 'Run code on demand with no servers to manage, scaling to zero (Azure Functions, Lambda).' },
  { term: 'Secret', category: 'Infrastructure', definition: 'Sensitive config (keys, passwords) kept out of code, in a vault or env var.' },
]
