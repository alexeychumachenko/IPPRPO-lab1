pipeline {
    agent any

    tools {
        dotnetsdk 'dotnet-8'
    }

    environment {
        PROJECT_DIR   = 'D:/LAB/lab3/MyLab1Project'
        TESTS_DIR     = 'D:/LAB/lab3/MyLab1Project.Tests'
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
                bat "dotnet build "${PROJECT_DIR}/MyLab1Project.csproj\" -c ${CONFIGURATION} --no-restore"
                bat "dotnet build "${TESTS_DIR}/MyLab1Project.Tests.csproj\" -c ${CONFIGURATION} --no-restore"
            }
        }

        stage('Test') {
            steps {
                bat "dotnet test "${TESTS_DIR}/MyLab1Project.Tests.csproj\" -c ${CONFIGURATION} --no-build --logger \"trx;LogFileName=test-results.trx\""
            }
            post {
                always {
                    step([$class: 'MSTestPublisher',
                          testResultsFile: "**/TestResults/*.trx",
                          failOnError: true,
                          keepLongStdio: true])
                }
            }
        }

        stage('Publish') {
            steps {
                bat "dotnet publish "${PROJECT_DIR}/MyLab1Project.csproj\" -c ${CONFIGURATION} -o \"${WORKSPACE}/publish\""
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
            // Очистка не требуется; отчёты уже опубликованы в стадии Test
        }
    }
}
