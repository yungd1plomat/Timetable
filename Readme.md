# Исходники бота для просмотра расписания БГПУ им. Акмуллы
[Ссылка на бота](https://vk.com/adobot)

## Сборка и публикация

### Сборка
1. Скачайте [Visual Studio 2022](https://visualstudio.microsoft.com/ru/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&passive=false&cid=2030), при установке выберите .NET 6 и Asp Net Core 6
2. Откройте проект, переименуйте appsettings_example.json в appsettings.json и заполните данные
3. Отредактируйте данные подключения к Mysql в файле DatabaseContext.cs
3. Выберите *Сборка - Собрать решение*

### Публикация
1. Установите [Docker](https://www.docker.com/products/docker-desktop/) 
2. Откройте проект в Visual Studio, выберите *Сборка - Опубликовать Timetable - Создать профиль - Реестр контейнеров Docker* и опубликуйте образ в нужном вам месте
3*. На целевой машине пробросьте порты на 3810 с помощью Ngrok для безопастности
4. Запустите проект при помощи команды запуск docker run --restart=always -itd -p 3810:3810 yungd1plomat/timetable
* - не обязательно