# Исходники бота для просмотра расписания БГПУ им. Акмуллы
[Ссылка на бота](https://vk.com/adobot)

## Возможности
1. Автопарсинг расписания с [АСУ БГПУ](https://asu.bspu.ru/Rasp/)
2. Возможность просмотреть расписание на сегодня, завтра и послезавтра
3. Поиск расписания по ключевым словам
4. Уведомление за указанное кол-во времени (задается пользователем)
5. Возможность подключить подписку при помощи [P2P QIWI](https://p2p.qiwi.com/)
6. Админские команды и статистика
7. Генерация изображения с расписанием на текущую и следующую неделю

## Сборка и публикация

### Сборка
1. Скачайте [Visual Studio 2022](https://visualstudio.microsoft.com/ru/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&passive=false&cid=2030), при установке выберите .NET 6 и Asp Net Core 6
2. Откройте проект, переименуйте appsettings_example.json в appsettings.json и заполните данные
3. Отредактируйте данные подключения к Mysql в файле DatabaseContext.cs
3. Выберите *Сборка - Собрать решение*

### Публикация
1. Установите [Docker](https://www.docker.com/products/docker-desktop/) 
2. Откройте проект в Visual Studio, выберите *Сборка - Опубликовать Timetable - Реестр контейнеров Docker* и опубликуйте образ в нужном вам месте
3. Либо же соберите образ вручную:
```docker build -t yungd1plomat/timetable:lastest
docker push yungd1plomat/timetable:latest```
4.\* На целевой машине пробросьте порты на 3810 с помощью Ngrok для безопастности
5. Запустите проект при помощи команды запуск docker run --restart=always -itd -p 3810:3810 yungd1plomat/timetable
\* - не обязательно

