USE PetShopDB;
GO

-- Insert Pets if not exists
INSERT INTO Pets (Name, Breed, Category, AgeMonths, Price, Description, ImageUrl, Status, CreatedAt)
VALUES
(N'Lucky', N'Shiba Inu', 'Dog', 4, 11500000, N'Shiba Inu Nhật Bản thuần chủng, mặt cười đáng yêu, lanh lợi, tính cách độc lập và trung thành tuyệt đối.', 'https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Mochi', N'Samoyed Tuyết Trắng', 'Dog', 3, 13500000, N'Samoyed lông trắng như tuyết, nụ cười thiên thần, cực kỳ quấn chủ và thân thiện với trẻ nhỏ.', 'https://images.unsplash.com/photo-1529429617124-95b109e86bb8?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Bino', N'Pug Mặt Xệ', 'Dog', 2, 5500000, N'Pug mặt xệ màu vàng kim, đôi mắt tròn xoe ngây thơ, tính tình hiền lành, thích được ôm ấp.', 'https://images.unsplash.com/photo-1517849845537-4d257902454a?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Rex', N'Becgie Đức (GSD)', 'Dog', 4, 9800000, N'Chó chăn cừu Đức thông minh đỉnh cao, vóc dáng vạm vỡ, can đảm, thích hợp huấn luyện bảo vệ.', 'https://images.unsplash.com/photo-1589941013453-ec89f33b5e95?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Cooper', N'Poodle Teacup Trắng', 'Dog', 2, 7200000, N'Poodle Teacup siêu nhỏ nhắn, lông xoăn trắng muốt như kẹo bông, đã biết đi vệ sinh đúng chỗ.', 'https://images.unsplash.com/photo-1546527868-ccb7ee7dfa6a?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Bông', N'Mèo Ba Tư Persian', 'Cat', 3, 6800000, N'Mèo Ba Tư lông xù quý tộc, mặt tịt đáng yêu, tính cách trầm tĩnh, dịu dàng, thích vuốt ve.', 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Simba', N'Mèo Bengal Vân Báo', 'Cat', 3, 15000000, N'Mèo Bengal hoa văn da báo hoang dã, thông minh, năng động, bơi lội giỏi, thần thái quý phái.', 'https://images.unsplash.com/photo-1513360309081-38f076278f1d?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Kiki', N'Mèo Xiêm Siamese', 'Cat', 4, 5200000, N'Mèo Xiêm mắt xanh biếc hút hồn, vóc dáng thanh mảnh kiêu sa, thông minh và thích giao tiếp.', 'https://images.unsplash.com/photo-1535930891776-0c2dfb7fda1a?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Nấm', N'Mèo Munchkin Chân Ngắn', 'Cat', 2, 10500000, N'Mèo Munchkin 4 chân ngắn ngủn lon ton, lông màu bicolor, tính cách nhí nhảnh vô cùng dễ thương.', 'https://images.unsplash.com/photo-1561948955-570b270e7c36?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Rio', N'Vẹt Macaw Nam Mỹ', 'Bird', 8, 25000000, N'Vẹt Macaw Nam Mỹ tuyệt đẹp, thông minh vượt trội, đã học được hơn 10 từ tiếng Việt, sống lâu tới 50 năm.', 'https://images.unsplash.com/photo-1544717305-2782549b5136?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Chirpy', N'Vẹt Yến Phụng Budgie', 'Bird', 3, 4500000, N'Vẹt Yến Phụng lông xanh ngọc bích, nhỏ nhắn nhanh nhẹn, hót líu lo cả ngày mang lại niềm vui cho gia đình.', 'https://images.unsplash.com/photo-1506220926639-b5ba770e0501?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Ruby', N'Cá La Hán Kim Cương', 'Fish', 5, 3200000, N'Cá La Hán gù to tròn, châu sáng lấp lánh như kim cương, dáng bơi uy nghiêm, biểu tượng phong thủy tài lộc.', 'https://images.unsplash.com/photo-1522069169874-c58ec4b76be5?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Goldie', N'Cá Betta Halfmoon Dumbo', 'Fish', 4, 350000, N'Cá xiêm đá Betta đuôi xòe 180 độ Halfmoon, tai bơi Dumbo to rộng, sắc đỏ ánh kim rực rỡ tuyệt mỹ.', 'https://images.unsplash.com/photo-1534447677768-be436bb09401?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE()),
(N'Cottontail', N'Thỏ Minilop Thuần Chủng', 'Rabbit', 3, 1600000, N'Thỏ Minilop tai cụp màu xám khói, bộ lông tơ mềm mượt như nhung, tính cách hiền hòa và sạch sẽ.', 'https://images.unsplash.com/photo-1518796745738-41048802f99a?auto=format&fit=crop&w=800&q=80', 'Available', GETUTCDATE());
GO

-- Insert Products
INSERT INTO Products (Name, Description, Price, Stock, Category, ImageUrl, CreatedAt, IsActive)
VALUES
(N'Pate Mèo Ciao Churu vị ức gà & cá ngừ', N'Pate dạng kem thơm ngon bổ dưỡng cho mèo mọi lứa tuổi, kích thích ăn uống ngon miệng.', 85000, 250, 'Food', 'https://images.unsplash.com/photo-1589924691995-400dc9ecc119?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Xương gặm sạch răng Dental Bổ sung Canxi', N'Xương nhai làm sạch mảng bám trên răng cho chó, chống hôi miệng và bổ sung canxi chắc xương.', 75000, 140, 'Food', 'https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Thức ăn hạt Orijen Six Fish cho mèo', N'Thức ăn cao cấp chiết xuất 6 loại cá tươi tự nhiên từ biển Canada, giàu đạm và axit béo omega.', 520000, 60, 'Food', 'https://images.unsplash.com/photo-1568640347023-a616a30bc3bd?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Trụ cào móng mèo 3 tầng bọc dây thừng tự nhiên', N'Trụ cào móng tích hợp nhà cây 3 tầng vững chãi, bọc dây sisal tự nhiên bền bỉ chống cào phá đồ.', 320000, 35, 'Toy', 'https://images.unsplash.com/photo-1545249390-6bdfa286032f?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Chuột đồ chơi lò xo phát nhạc', N'Đồ chơi gắn lò xo lắc lư kích thích bản năng rượt đuổi, chất liệu bông mềm an toàn cho mèo con.', 65000, 80, 'Toy', 'https://images.unsplash.com/photo-1533738363-b7f9aef128ce?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Đệm tròn lông cừu êm ái cho chó mèo size L', N'Đệm nằm siêu mềm mại ấm áp, bông PP đàn hồi cao cấp, đáy chống trượt và chống thấm ẩm.', 260000, 40, 'Accessory', 'https://images.unsplash.com/photo-1541599540903-216a46ca1dc0?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Máy lọc nước tự động cho thú cưng 2.5L', N'Máy uống nước tuần hoàn tự động kèm màng lọc ion đa tầng, siêu êm, kích thích thú cưng uống nước.', 380000, 55, 'Accessory', 'https://images.unsplash.com/photo-1591946614720-90a587da4a36?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Xịt khử mùi diệt khuẩn chuồng trại & khay cát', N'Dung dịch xịt khử mùi hôi chuồng trại và khay cát sinh học, hương hoa nhài thanh mát dịu nhẹ.', 110000, 90, 'Grooming', 'https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Kìm cắt móng thú cưng kèm dũa móng an toàn', N'Kìm cắt móng lưỡi thép không gỉ sắc bén, có vách ngăn cữ chống cắt phạm tủy máu móng.', 85000, 110, 'Grooming', 'https://images.unsplash.com/photo-1516734212186-a967f81ad0d7?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1),
(N'Gel dinh dưỡng Megaderm dưỡng lông mượt mà', N'Gel bổ sung axit béo thiết yếu Omega 3 & 6, giảm rụng lông, phục hồi da nhạy cảm dị ứng.', 215000, 65, 'Medicine', 'https://images.unsplash.com/photo-1628088062854-d1870b4553da?auto=format&fit=crop&w=800&q=80', GETUTCDATE(), 1);
GO

