//------------------------------------------------------------------------------
#ifndef MUTEX_H_INCLUDED
#define MUTEX_H_INCLUDED
//------------------------------------------------------------------------------
#include <windows.h>
//------------------------------------------------------------------------------
class Mutex
{
public:
  Mutex();
  virtual ~Mutex();

private:
  Mutex(const Mutex& other) {}
  Mutex& operator=(const Mutex& other) { return *this; }

public:
  void Request() const;
  void Release() const;

private:
  HANDLE m_hMutex;
};
//------------------------------------------------------------------------------
#endif
//------------------------------------------------------------------------------
