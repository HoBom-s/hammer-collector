@Library('hobom-shared-lib') _
hobomPipeline(
  serviceName:    'dev-hammer-collector',
  hostPort:       '5003',
  containerPort:  '8080',
  memory:         '512m',
  cpus:           '0.5',
  envPath:        '/etc/hobom-dev/dev-hammer-collector/.env',
  addHost:        true,
  submodules:     false,
  smokeCheckPath: '/health'
)
