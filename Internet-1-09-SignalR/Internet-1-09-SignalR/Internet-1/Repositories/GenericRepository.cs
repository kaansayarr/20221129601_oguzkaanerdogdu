﻿using Internet_1.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Internet_1.Repositories
{
    public class GenericRepository<T> where T : class
    {
        public readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<LessonVideo>> GetByIdsAsync(List<int> ids)
        {
            return await _context.Videos.Where(v => ids.Contains(v.Id)).ToListAsync();
        }



        public async Task DeleteLessonVideos(List<LessonVideo> lessonVideos)
        {
            _context.Videos.RemoveRange(lessonVideos);
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return _dbSet.Where(expression);
        }
    }
}
