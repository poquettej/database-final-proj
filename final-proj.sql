create database finalproj;
use finalproj;

create table user (
user_id int primary key auto_increment,
username varchar(32) not null,
user_email varchar(128) not null,
pass_hash varchar(255) not null
);


create table game (
game_id int primary key auto_increment,
game_title varchar(128) not null,
game_summary text,
cover_url varchar(256),
release_date timestamp,
fulltext (game_title, game_summary)
);

create table website (
website_id int primary key,
game_id int not null,
website_url varchar(256) not null,
foreign key (game_id) references game(game_id)
);

create table company (
company_id int primary key,
company_name varchar(128)
);

create table involved_in (
game_id int not null,
company_id int not null,
is_developer boolean not null,
is_publisher boolean not null,
primary key (game_id, company_id),
foreign key (game_id) references game(game_id),
foreign key (company_id) references company(company_id)
);

create table review (
review_id int primary key auto_increment,
user_id int not null,
game_id int not null,
rating int not null,
review_comment text,
date_reviewed timestamp,
foreign key (user_id) references user(user_id),
foreign key (game_id) references game(game_id)
);

create table bookmark (
user_id int not null,
game_id int not null,
date_bookmarked timestamp,
primary key (user_id, game_id),
foreign key (user_id) references user(user_id),
foreign key (game_id) references game(game_id)
);

create table platform (
platform_id int primary key,
platform_name varchar(128) not null
);

create table supported_platforms (
game_id int not null,
platform_id int not null,
download_page_url text,
foreign key (game_id) references game(game_id),
foreign key (platform_id) references platform(platform_id),
primary key (game_id, platform_id)
);
