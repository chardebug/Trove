Create Table If Not Exists Video
(
    video_id integer PRIMARY KEY,
    file_path varchar(255),
    title varchar(255),
    thumbnail_path varchar(255),
    video_length varchar(255),
    file_size integer,
    video_format varchar(255),
    description varchar(255)
);


Create Table If Not Exists Tags
(
    tag_name varchar(255) PRIMARY KEY

);

Create Table If Not Exists Video_Tags
(
    video_tag_id integer PRIMARY KEY,
    video_id Integer,
    tag_name varchar(255),
    
    FOREIGN KEY (video_id) REFERENCES
    Video(video_id),
    FOREIGN KEY (tag_name) REFERENCES
    Tags(tag_name)
);