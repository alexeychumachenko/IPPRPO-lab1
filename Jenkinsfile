pipeline {
    agent any

    tools {
        // Имя должно совпадать с настройкой в Jenkins: Manage Jenkins → Global Tool Configuration
        dotnetsdk 'dotnet-8'
    }

    environment {
        PROJECT_DIR   = 'MyLab1Project'
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
                bat "dotnet build ${PROJECT_DIR}/MyLab1Project.csproj -c ${CONFIGURATION} --no-restore"
            }
        }

        stage('Test') {
            steps {
                bat "dotnet test ${TESTS_DIR}/MyLab1Project.Tests.csproj -c ${CONFIGURATION} --logger \"trx;LogFileName=test-results.trx\""
            }
            /*
            post {
                always {
                    junit 'test-results.trx'
                }
            }
            */
        }
        
        stage('Publish') {
            steps {
                bat "dotnet publish MyLab1Project/MyLab1Project.csproj -c ${CONFIGURATION} -o ${WORKSPACE}/publish"
                echo 'Сохранение артефакта...'
                // Создаём папку artifacts и файл build.txt (Windows-синтаксис)
                bat "if not exist artifacts mkdir artifacts"
                bat "echo Build %BUILD_NUMBER% > artifacts\\build.txt"
            }
            post {
                success {
                    archiveArtifacts artifacts: 'artifacts/**', fingerprint: true
                }
            }
        }
    }

    post {
        always {
                step([$class: 'MSTestPublisher', testResultsFile: "**/TestResults/*.trx", failOnError: false, keepLongStdio: true])
        }
    }
}
