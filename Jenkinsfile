pipeline {
    agent any

    tools {
        // Имя должно совпадать с настройкой в Jenkins: Manage Jenkins → Global Tool Configuration
        dotnetsdk 'dotnet-8'
    }

    environment {
        PROJECT_DIR   = 'lab3/MyLab1Project'
        TESTS_DIR     = 'lab3/MyLab1Project.Tests'
        CONFIGURATION = 'Release'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                bat "dotnet restore ${PROJECT_DIR}/MyLab1Project.csproj"
                bat "dotnet restore ${TESTS_DIR}/MyLab1Project.Tests.csproj"
            }
        }

        stage('Build') {
            steps {
                bat "dotnet build ${PROJECT_DIR}/MyLab1Project.csproj -c ${CONFIGURATION} --no-restore"
            }
        }

        stage('Test') {
            steps {
                bat "dotnet test ${TESTS_DIR}/MyLab1Project.Tests.csproj -c ${CONFIGURATION} --no-build --logger \"trx;LogFileName=test-results.trx\""
            }
            post {
                always {
                    junit '**/TestResults/*.trx'
                }
            }
        }

        stage('Publish') {
            steps {
                bat "dotnet publish ${PROJECT_DIR}/MyLab1Project.csproj -c ${CONFIGURATION} -o ${WORKSPACE}/publish"
            }
            post {
                success {
                    archiveArtifacts artifacts: 'publish/**/*', fingerprint: true
                }
            }
        }
    }

    post {
        always {
            cleanWs()
        }
    }
}
