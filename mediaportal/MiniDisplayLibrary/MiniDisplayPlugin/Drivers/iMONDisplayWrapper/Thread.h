//------------------------------------------------------------------------------
#ifndef THREAD_H_INCLUDED
#define THREAD_H_INCLUDED
//------------------------------------------------------------------------------
#include <windows.h>
//------------------------------------------------------------------------------
class Thread
{
protected:
  Thread();
  virtual ~Thread();

private:
  Thread(const Thread& other) {}
  Thread& operator=(const Thread& other) { *this; }

public:
  void Start();
  virtual void Interrupt() = 0;
  void Join() const;  

protected:
  virtual void Run() = 0;

private:
  static DWORD WINAPI ThreadProc(LPVOID lpParam);

private:
  HANDLE m_hThread;
  DWORD m_dwThreadId;
};
//------------------------------------------------------------------------------
#endif
//------------------------------------------------------------------------------
